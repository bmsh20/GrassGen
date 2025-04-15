using Godot;
using System;


[Tool]
public partial class GrassGenDemo : Node3D
{

    [Export]
    public GrassGenConfig Configuration { get; set; }
    
    private GrassGenInitializer renderer;
    private GrassGenCompute computeShader;
    

    [Export]
    public int GrassCount 
    { 
        get => Configuration?.GrassCount ?? 10000; 
        set 
        { 
            if (Configuration != null) 
                Configuration.GrassCount = value; 
        } 
    }
    

    // size in square
    [Export] 
    public float AreaSize 
    { 
        get => Configuration?.AreaSize ?? 10.0f; 
        set 
        { 
            if (Configuration != null) 
                Configuration.AreaSize = value; 
        } 
    }
    

    [Export]
    public float MinScale 
    { 
        get => Configuration?.MinScale ?? 0.7f; 
        set 
        { 
            if (Configuration != null) 
                Configuration.MinScale = value; 
        } 
    }

    [Export]
    public float MaxScale 
    { 
        get => Configuration?.MaxScale ?? 1.3f; 
        set 
        { 
            if (Configuration != null) 
                Configuration.MaxScale = value; 
        } 
    }
    

    [Export]
    public Mesh GrassMesh 
    { 
        get => Configuration?.GrassMesh; 
        set 
        { 
            if (Configuration != null) 
                Configuration.GrassMesh = value; 
        } 
    }

    [Export]
    public Material GrassMaterial 
    { 
        get => Configuration?.GrassMaterial; 
        set 
        { 
            if (Configuration != null) 
                Configuration.GrassMaterial = value; 
        } 
    }
    


    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
            
        EnsureConfiguration();
        InitializeComponents();
        GenerateGrass();
    }
    

    private void EnsureConfiguration()
    {
        if (Configuration == null)
        {
            Configuration = new GrassGenConfig();
        }
    }
    
    private void InitializeComponents()
    {
        EnsureConfiguration();
        
        renderer = new GrassGenInitializer(Configuration);
        renderer.Initialize(this);
        
        computeShader = new GrassGenCompute(Configuration);
    }
    

    // Data flow:
    // updates the instance count in the renderer
    // calls GrassGenCompute to generate transforms on GPU
    // reads each transform from the raw data
    // sends each transform to the renderer to be applied to the MultiMesh

    private void GenerateGrass()
    {
        if (renderer == null || computeShader == null)
        {
            InitializeComponents();
        }
        
        if (!renderer.IsReady())
            return;
        
        renderer.UpdateInstanceCount(Configuration.GrassCount);
        
        var data = computeShader.GenerateTransforms();
        
        for (int i = 0; i < Configuration.GrassCount; i++)
        {
            var transform = computeShader.ReadTransform(data, i * 16 * sizeof(float));
            renderer.SetInstanceTransform(i, transform);
        }
        
        GD.Print($"generated {Configuration.GrassCount} grass instances");
    }
    
    public void UpdateGrass()
    {
        InitializeComponents();
        GenerateGrass();
    }
    
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && Input.IsActionJustPressed("ui_accept"))
        {
            UpdateGrass();
        }
    }
}