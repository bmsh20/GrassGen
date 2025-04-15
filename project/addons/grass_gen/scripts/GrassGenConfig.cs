using Godot;
using System;


[Tool]
[GlobalClass]
public partial class GrassGenConfig : Resource
{

    // data container class 
    // default vals that work ok

    [Export]
    public int GrassCount { get; set; } = 10000;
    

    [Export] 
    public float AreaSize { get; set; } = 10.0f;
    
    [Export]
    public float MinScale { get; set; } = 0.7f;
    

    [Export]
    public float MaxScale { get; set; } = 1.3f;
    
    [Export]
    public Mesh GrassMesh { get; set; }
    
    [Export]
    public Material GrassMaterial { get; set; }
} 