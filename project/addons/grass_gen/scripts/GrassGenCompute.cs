using Godot;
using System;

[Tool]
public partial class GrassGenCompute : Node
{
    private RenderingDevice renderDevice;
    private GrassGenConfig config;
    
    public GrassGenCompute(GrassGenConfig config)
    {
        this.config = config;
    }
    
    public byte[] GenerateTransforms()
    {
        if (renderDevice == null)
        {
            renderDevice = RenderingServer.GetRenderingDevice();
        }
        
        var shader = GD.Load<RDShaderFile>("res://addons/grass_gen/shaders/grass_compute.glsl");
        var shaderRid = renderDevice.ShaderCreateFromSpirV(shader.GetSpirV());
        
        var grassCount = config.GrassCount;
        
        // comp shader output
        var outputBuffer = renderDevice.StorageBufferCreate((uint)(grassCount * 16 * sizeof(float)));
        
        // transform uni for compute to consume
        var transformUniform = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 0 };
        transformUniform.AddId(outputBuffer);
        

        // we do this for padding because Godot complains about it
        byte[] paramBytes = new byte[16];
        
        // write grassparams directly to the byte array
        BitConverter.GetBytes(config.AreaSize).CopyTo(paramBytes, 0);
        BitConverter.GetBytes(config.MinScale).CopyTo(paramBytes, 4);
        BitConverter.GetBytes(config.MaxScale).CopyTo(paramBytes, 8);
        // rest is left as zeros for padding (12-15 bytes)
        
        var paramBuffer = renderDevice.UniformBufferCreate(16, paramBytes);
        
        var paramUniform = new RDUniform { UniformType = RenderingDevice.UniformType.UniformBuffer, Binding = 1 };
        paramUniform.AddId(paramBuffer);
        
        var uniformArray = new Godot.Collections.Array<RDUniform>();
        uniformArray.Add(transformUniform);
        uniformArray.Add(paramUniform);
        var uniformSet = renderDevice.UniformSetCreate(uniformArray, shaderRid, 0);
        
        var pipeline = renderDevice.ComputePipelineCreate(shaderRid);
        var computeList = renderDevice.ComputeListBegin();
        renderDevice.ComputeListBindComputePipeline(computeList, pipeline);
        renderDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        renderDevice.ComputeListDispatch(computeList, (uint)((grassCount + 63) / 64), 1, 1);
        renderDevice.ComputeListEnd();
        
        renderDevice.Submit();
        renderDevice.Sync();
        
        var data = renderDevice.BufferGetData(outputBuffer);
        
        renderDevice.FreeRid(outputBuffer);
        renderDevice.FreeRid(paramBuffer);
        renderDevice.FreeRid(uniformSet);
        renderDevice.FreeRid(pipeline);
        renderDevice.FreeRid(shaderRid);
        
        return data;
    }
    
    // this could be trimmed down todo 
    public Transform3D ReadTransform(byte[] data, int offset)
    {
        var basis = new Basis(
            new Vector3(
                BitConverter.ToSingle(data, offset + 0 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 1 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 2 * sizeof(float))
            ),
            new Vector3(
                BitConverter.ToSingle(data, offset + 4 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 5 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 6 * sizeof(float))
            ),
            new Vector3(
                BitConverter.ToSingle(data, offset + 8 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 9 * sizeof(float)),
                BitConverter.ToSingle(data, offset + 10 * sizeof(float))
            )
        );
        
        var origin = new Vector3(
            BitConverter.ToSingle(data, offset + 12 * sizeof(float)),
            BitConverter.ToSingle(data, offset + 13 * sizeof(float)),
            BitConverter.ToSingle(data, offset + 14 * sizeof(float))
        );
        
        return new Transform3D(basis, origin);
    }
} 