namespace AnythingWorld.Utilities
{
    public enum AnimationPipeline
    {
        Unset,
        Static,
        Rigged,
        WheeledVehicle,
        PropellorVehicle,
        Shader
    }

    public enum ModelLoadingPipeline
    {
        Unset,
        RiggedGLB,
        GLTF,
        OBJ_Static,
        OBJ_Part_Based
    }

    /// <summary>
    /// User Input Scale Type
    /// </summary>
    /// <param name="SetRealWorld">Will scale model to fit inside a box of the provided scale.</param>
    /// <param name="ScaleRealWorld">Scales model to size provided by database, then scales top level transform by provided input.</param>
    /// <param name="Absolute">Model and rig set to input scale vector, top level transform not changed (EXPERIMENTAL).</param>
    public enum ScaleType
    {
        SetRealWorld,
        ScaleRealWorld,
        Absolute
    }

    public enum TransformSpace
    {
        Local,
        World
    }

    public enum RequestType
    {
        Search,
        Json
    }

    public enum SortingDropdownOption
    {
        MostRelevant, /*MostPopular,*/ MostLiked, MyList, AtoZ, ZtoA /*, Newest, Oldest*/
    }
}