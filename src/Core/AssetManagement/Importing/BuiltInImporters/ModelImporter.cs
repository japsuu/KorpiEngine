using Assimp;
using KorpiEngine.Animations;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.Utils;
using Animation = KorpiEngine.Animations.Animation;
using Material = KorpiEngine.Rendering.Material;
using Matrix4x4 = KorpiEngine.Mathematics.Matrix4x4;
using Mesh = KorpiEngine.Rendering.Mesh;
using Quaternion = KorpiEngine.Mathematics.Quaternion;
using Texture2D = KorpiEngine.Rendering.Texture2D;
using TextureType = Assimp.TextureType;

namespace KorpiEngine.AssetManagement;

[AssetImporter(".obj", ".blend", ".dae", ".fbx", ".gltf", ".ply", ".pmx", ".stl")]
public class ModelImporter : AssetImporter
{
    public bool GenerateColliders { get; set; } = false;
    public bool GenerateNormals { get; set; } = true;
    public bool GenerateSmoothNormals { get; set; } = false;
    public bool CalculateTangentSpace { get; set; } = true;
    public bool MakeLeftHanded { get; set; } = false;
    public bool FlipUVs { get; set; } = false;
    public bool RemoveEmptyEntities { get; set; } = false;
    public bool OptimizeGraph { get; set; } = false;
    public bool OptimizeMeshes { get; set; } = false;
    public bool FlipWindingOrder { get; set; } = false;
    public bool WeldVertices { get; set; } = false;
    public bool InvertNormals { get; set; } = false;
    public bool GlobalScale { get; set; } = false;

    public float UnitScale { get; set; } = 1.0f;


    public override Asset Import(FileInfo assetPath)
    {
        using AssimpContext importer = new();
        
        Entity entity = ImportAssimpEntity(assetPath, importer);
        return entity;
    }


    private Entity ImportAssimpEntity(FileInfo assetPath, AssimpContext importer)
    {
        DirectoryInfo? parentDir = assetPath.Directory;
        
        if (parentDir == null)
            throw new AssetImportException<Mesh>($"Could not get parent directory of asset at {assetPath}");
        
        Scene? scene = LoadAssimpScene(assetPath, importer);
        if (scene == null)
            throw new AssetImportException<Mesh>($"Assimp returned null scene for asset at {assetPath}");

        if (!scene.HasMeshes)
            throw new AssetImportException<Mesh>($"No meshes found in scene for asset at {assetPath}");

        float scale = UnitScale;

        // FBX files are usually in cm, so scale them to meters
        if (assetPath.Extension.Equals(".fbx", StringComparison.OrdinalIgnoreCase))
            scale *= 0.01f;

        // Replicate the Assimp node hierarchy.
        // This creates an empty entity hierarchy with the same structure as the assimp scene.
        List<(Entity entity, Node node)> entityHierarchy = [];  //WARN: Replace with an actual tree structure
        string rootEntityName = $"{Path.GetFileNameWithoutExtension(assetPath.Name)} model";
        CreateEntityHierarchy(rootEntityName, scene.RootNode, ref entityHierarchy, scale);

        // Materials
        List<AssetRef<Material>> materials = [];
        if (scene.HasMaterials)
            LoadMaterials(scene, parentDir, materials);

        // Animations
        List<AssetRef<AnimationClip>> animations = [];
        if (scene.HasAnimations)
            animations = LoadAnimations(scene, scale);

        // Meshes. Also bind materials to meshes
        List<MeshMaterialBinding> meshMaterialBindings = [];
        if (scene.HasMeshes)
            LoadMeshes(assetPath, scene, scale, materials, meshMaterialBindings);

        // Create child entities for each mesh in hierarchy.
        foreach ((Entity? entity, Node? node) in entityHierarchy)
        {
            if (!node.HasMeshes)
                continue;
            
            if (node.MeshIndices.Count == 1)
            {
                // Just a single mesh, we can use the node "root". No need to create children.
                int meshIndex = node.MeshIndices[0];
                MeshMaterialBinding meshMatBinding = meshMaterialBindings[meshIndex];
                AddMeshComponent(entityHierarchy, entity, meshMatBinding);
            }
            else
            {
                // Multiple meshes, create child for each mesh.
                foreach (int meshIndex in node.MeshIndices)
                {
                    MeshMaterialBinding meshMatBinding = meshMaterialBindings[meshIndex];
                    Entity child = new(null, meshMatBinding.MeshName);
                    
                    AddMeshComponent(entityHierarchy, child, meshMatBinding);
                    child.SetParent(entity, false);
                }
            }
        }

        Entity rootEntity = entityHierarchy[0].entity;
        if (!Mathematics.MathOps.AlmostEquals(UnitScale, 1.0f))
            rootEntity.Transform.LocalScale = Vector3.One * UnitScale;

        // Add Animation Component to root, with all the animations assigned to it.
        if (animations.Count > 0)
        {
            Animation anim = rootEntity.AddComponent<Animation>();
            foreach (AssetRef<AnimationClip> a in animations)
                anim.Clips.Add(a);
            anim.DefaultClip = animations[0];
        }

        if (!RemoveEmptyEntities)
            return rootEntity;

        List<(Entity entity, Node node)> entitiesToRemove = [];
                
        foreach ((Entity entity, Node node) pair in entityHierarchy)
        {
            if (!pair.entity.GetComponentsInChildren<EntityComponent>().Any())
                entitiesToRemove.Add(pair);
        }
                
        foreach ((Entity entity, Node node) pair in entitiesToRemove)
        {
            if (!pair.entity.IsDestroyed)
                pair.entity.DestroyImmediate();
            entityHierarchy.Remove(pair);
        }

        return rootEntity;
    }


    private Scene? LoadAssimpScene(FileInfo assetPath, AssimpContext importer)
    {
        importer.SetConfig(new Assimp.Configs.VertexBoneWeightLimitConfig(4));
        
        PostProcessSteps steps =
            PostProcessSteps.LimitBoneWeights |
            PostProcessSteps.GenerateUVCoords |
            PostProcessSteps.Triangulate;
        
        if (GenerateNormals)
        {
            if (GenerateSmoothNormals)
                steps |= PostProcessSteps.GenerateSmoothNormals;
            else
                steps |= PostProcessSteps.GenerateNormals;
        }
        
        if (CalculateTangentSpace)
            steps |= PostProcessSteps.CalculateTangentSpace;
        
        if (MakeLeftHanded)
            steps |= PostProcessSteps.MakeLeftHanded;
        
        if (FlipUVs)
            steps |= PostProcessSteps.FlipUVs;
        
        if (OptimizeGraph)
            steps |= PostProcessSteps.OptimizeGraph;
        
        if (OptimizeMeshes)
            steps |= PostProcessSteps.OptimizeMeshes;
        
        if (FlipWindingOrder)
            steps |= PostProcessSteps.FlipWindingOrder;
        
        if (WeldVertices)
            steps |= PostProcessSteps.JoinIdenticalVertices;
        
        if (GlobalScale)
            steps |= PostProcessSteps.GlobalScale;
        
        return importer.ImportFile(assetPath.FullName, steps);
    }


    private static Entity CreateEntityHierarchy(string? name, Node assimpNode, ref List<(Entity entity, Node node)> hierarchy, float scaleFactor, int i = 0)
    {
        Entity entity = new(null, name ?? assimpNode.Name);
        hierarchy.Add((entity, assimpNode));
        entity.Name = name ?? assimpNode.Name;

        if (assimpNode.HasChildren)
        {
            foreach (Node? cn in assimpNode.Children)
            {
                Entity go = CreateEntityHierarchy(null, cn, ref hierarchy, scaleFactor, i + 1);
                go.SetParent(entity, false);
            }
        }

        // Transform
        Assimp.Matrix4x4 t = assimpNode.Transform;
        t.Decompose(out Vector3D aSca, out Assimp.Quaternion aRot, out Vector3D aPos);

        entity.Transform.LocalPosition = new Vector3(aPos.X, aPos.Y + i, aPos.Z) * scaleFactor;
        entity.Transform.LocalRotation = new Quaternion(aRot.X, aRot.Y, aRot.Z, aRot.W);
        entity.Transform.LocalScale = new Vector3(aSca.X, aSca.Y, aSca.Z);

        return entity;
    }


    #region Material loading

    private static void LoadMaterials(Scene scene, DirectoryInfo parentDir, List<AssetRef<Material>> mats)
    {
        foreach (Assimp.Material? sourceMat in scene.Materials)
        {
            Material targetMat = new(Shader.Find("Assets/Defaults/Standard.kshader"), "standard material");
            targetMat.Name = sourceMat.HasName ? sourceMat.Name : "Standard Material";

            // Diffuse color (main color)
            LoadAssimpDiffuseColor(sourceMat, targetMat);

            // Emissive Color
            LoadAssimpEmission(sourceMat, targetMat);

            // Diffuse
            LoadAssimpDiffuse(parentDir, sourceMat, targetMat);

            // Normal
            LoadAssimpNormal(parentDir, sourceMat, targetMat);

            // AO, Roughness, Metallic
            LoadAssimpSurface(parentDir, sourceMat, targetMat);

            // Emissive
            LoadAssimpEmissive(parentDir, sourceMat, targetMat);

            mats.Add(targetMat);
        }
    }


    private static void LoadAssimpDiffuseColor(Assimp.Material sourceMat, Material targetMat)
    {
        ColorHDR diffColor = sourceMat.HasColorDiffuse ? new ColorHDR(sourceMat.ColorDiffuse.R, sourceMat.ColorDiffuse.G, sourceMat.ColorDiffuse.B, sourceMat.ColorDiffuse.A) : ColorHDR.White;
        targetMat.SetColor("_MainColor", diffColor);
    }


    private static void LoadAssimpEmission(Assimp.Material sourceMat, Material targetMat)
    {
        if (sourceMat.HasColorEmissive)
        {
            targetMat.SetFloat("_EmissionIntensity", 1f);
            targetMat.SetColor("_EmissiveColor", new ColorHDR(sourceMat.ColorEmissive.R, sourceMat.ColorEmissive.G, sourceMat.ColorEmissive.B, sourceMat.ColorEmissive.A));
        }
        else
        {
            targetMat.SetFloat("_EmissionIntensity", 0f);
            targetMat.SetColor("_EmissiveColor", ColorHDR.Black);
        }
    }


    private static void LoadAssimpDiffuse(DirectoryInfo parentDir, Assimp.Material sourceMat, Material targetMat)
    {
        if (!sourceMat.HasTextureDiffuse)
            return;
        
        if (TryFindTextureFromPath(sourceMat.TextureDiffuse.FilePath, parentDir, out FileInfo? file))
            LoadTextureIntoMesh(Material.MAIN_TEX, file, targetMat);
    }


    private static void LoadAssimpNormal(DirectoryInfo parentDir, Assimp.Material sourceMat, Material targetMat)
    {
        if (!sourceMat.HasTextureNormal)
            return;
        
        if (TryFindTextureFromPath(sourceMat.TextureNormal.FilePath, parentDir, out FileInfo? file))
            LoadTextureIntoMesh(Material.NORMAL_TEX, file, targetMat);
    }


    private static void LoadAssimpSurface(DirectoryInfo parentDir, Assimp.Material sourceMat, Material targetMat)
    {
        if (!sourceMat.GetMaterialTexture(TextureType.Unknown, 0, out TextureSlot surface))
            return;
        
        if (TryFindTextureFromPath(surface.FilePath, parentDir, out FileInfo? file))
            LoadTextureIntoMesh(Material.SURFACE_TEX, file, targetMat);
    }


    private static void LoadAssimpEmissive(DirectoryInfo parentDir, Assimp.Material sourceMat, Material targetMat)
    {
        if (!sourceMat.HasTextureEmissive)
            return;

        if (!TryFindTextureFromPath(sourceMat.TextureEmissive.FilePath, parentDir, out FileInfo? file))
            return;
        
        targetMat.SetFloat("_EmissionIntensity", 1f);
        LoadTextureIntoMesh(Material.EMISSION_TEX, file, targetMat);
    }
    
    
    private static bool TryFindTextureFromPath(string filePath, DirectoryInfo parentDir, out FileInfo file)
    {
        // If the filePath is stored in the model relative to the file, this will exist.
        file = new FileInfo(Path.Combine(parentDir.FullName, filePath));
        if (File.Exists(file.FullName))
            return true;

        // The filePath is probably an absolute path, so lets loop over each node in the path starting from the end.
        // First check if the file exists inside parentDir, if so return.
        // If not, try the next node in the path.
        string[] nodes = filePath.Split(Path.DirectorySeparatorChar);
        for (int i = nodes.Length - 1; i >= 0; i--)
        {
            string path = Path.Combine(parentDir.FullName, string.Join(Path.DirectorySeparatorChar, nodes.Skip(i)));
            file = new FileInfo(path);
            if (file.Exists)
                return true;
        }

        // If we get here, we have failed to find the texture.
        return false;
    }


    private static void LoadTextureIntoMesh(string name, FileInfo file, Material mat)
    {
        if (AssetManager.TryGetGuidFromPath(file, out UUID guid))
        {
            // We have this texture as an asset, use the asset, we don't need to load it
            mat.SetTexture(name, new AssetRef<Texture2D>(guid));
        }
        else
        {
            // Import external textures
            string relativePath = AssetManager.ToRelativePath(file);
            
            if (!file.Exists)
                Application.Logger.Error($"Texture file '{file.FullName}' missing, skipping...");
            
            mat.SetTexture(name, AssetManager.LoadAssetFile<Texture2D>(relativePath));
        }
    }

    #endregion


    #region Animation loading

    private static List<AssetRef<AnimationClip>> LoadAnimations(Scene scene, float scale)
    {
        List<AssetRef<AnimationClip>> anims = [];
        foreach (Assimp.Animation? anim in scene.Animations)
        {
            AnimationClip animation = LoadAssimpAnimation(scene, scale, anim);
            anims.Add(animation);
        }

        return anims;
    }


    private static AnimationClip LoadAssimpAnimation(Scene scene, float scale, Assimp.Animation sourceAnim)
    {
        // Create Animation
        AnimationClip destinationAnim = new();
        destinationAnim.Name = sourceAnim.Name;
        destinationAnim.Duration = (float)sourceAnim.DurationInTicks / (Mathematics.MathOps.AlmostEquals((float)sourceAnim.TicksPerSecond, 0f) ? 25.0f : (float)sourceAnim.TicksPerSecond);
        destinationAnim.TicksPerSecond = (float)sourceAnim.TicksPerSecond;
        destinationAnim.DurationInTicks = (float)sourceAnim.DurationInTicks;

        foreach (NodeAnimationChannel? channel in sourceAnim.NodeAnimationChannels)
        {
            Node boneNode = scene.RootNode.FindNode(channel.NodeName);

            AnimationClip.AnimBone animBone = new()
            {
                BoneName = boneNode.Name
            };

            if (channel.HasPositionKeys)
                LoadAssimpAnimationPositionKeys(scale, channel, sourceAnim, destinationAnim, animBone);

            if (channel.HasRotationKeys)
                LoadAssimpAnimationRotationKeys(channel, sourceAnim, destinationAnim, animBone);

            if (channel.HasScalingKeys)
                LoadAssimpAnimationScalingKeys(channel, sourceAnim, destinationAnim, animBone);

            destinationAnim.AddBone(animBone);
        }

        destinationAnim.EnsureQuaternionContinuity();
        return destinationAnim;
    }


    private static void LoadAssimpAnimationPositionKeys(float scale, NodeAnimationChannel channel, Assimp.Animation sourceAnim, AnimationClip destinationAnim, AnimationClip.AnimBone animBone)
    {
        AnimationCurve xCurve = new();
        AnimationCurve yCurve = new();
        AnimationCurve zCurve = new();
        foreach (VectorKey posKey in channel.PositionKeys)
        {
            double time = posKey.Time / sourceAnim.DurationInTicks * destinationAnim.Duration;
            xCurve.Keys.Add(new KeyFrame((float)time, posKey.Value.X * scale));
            yCurve.Keys.Add(new KeyFrame((float)time, posKey.Value.Y * scale));
            zCurve.Keys.Add(new KeyFrame((float)time, posKey.Value.Z * scale));
        }

        animBone.PosX = xCurve;
        animBone.PosY = yCurve;
        animBone.PosZ = zCurve;
    }


    private static void LoadAssimpAnimationRotationKeys(NodeAnimationChannel channel, Assimp.Animation sourceAnim, AnimationClip destinationAnim, AnimationClip.AnimBone animBone)
    {
        AnimationCurve xCurve = new();
        AnimationCurve yCurve = new();
        AnimationCurve zCurve = new();
        AnimationCurve wCurve = new();
        foreach (QuaternionKey rotKey in channel.RotationKeys)
        {
            double time = rotKey.Time / sourceAnim.DurationInTicks * destinationAnim.Duration;
            xCurve.Keys.Add(new KeyFrame((float)time, rotKey.Value.X));
            yCurve.Keys.Add(new KeyFrame((float)time, rotKey.Value.Y));
            zCurve.Keys.Add(new KeyFrame((float)time, rotKey.Value.Z));
            wCurve.Keys.Add(new KeyFrame((float)time, rotKey.Value.W));
        }

        animBone.RotX = xCurve;
        animBone.RotY = yCurve;
        animBone.RotZ = zCurve;
        animBone.RotW = wCurve;
    }


    private static void LoadAssimpAnimationScalingKeys(NodeAnimationChannel channel, Assimp.Animation sourceAnim, AnimationClip destinationAnim, AnimationClip.AnimBone animBone)
    {
        AnimationCurve xCurve = new();
        AnimationCurve yCurve = new();
        AnimationCurve zCurve = new();
        foreach (VectorKey scaleKey in channel.ScalingKeys)
        {
            double time = scaleKey.Time / sourceAnim.DurationInTicks * destinationAnim.Duration;
            xCurve.Keys.Add(new KeyFrame((float)time, scaleKey.Value.X));
            yCurve.Keys.Add(new KeyFrame((float)time, scaleKey.Value.Y));
            zCurve.Keys.Add(new KeyFrame((float)time, scaleKey.Value.Z));
        }

        animBone.ScaleX = xCurve;
        animBone.ScaleY = yCurve;
        animBone.ScaleZ = zCurve;
    }

    #endregion


    #region Mesh loading

    private void LoadMeshes(FileInfo assetPath, Scene scene, double scale, List<AssetRef<Material>> mats, List<MeshMaterialBinding> meshMats)
    {
        foreach (Assimp.Mesh? assimpMesh in scene.Meshes)
        {
            if (assimpMesh.PrimitiveType != Assimp.PrimitiveType.Triangle)
            {
                Application.Logger.Info($"{assetPath.Name}'s mesh '{assimpMesh.Name}' is not of Triangle Primitive, Skipping...");
                continue;
            }

            Mesh engineMesh = LoadAssimpMesh(scale, assimpMesh);
            meshMats.Add(new MeshMaterialBinding(assimpMesh.Name, assimpMesh, engineMesh, mats[assimpMesh.MaterialIndex]));
        }
    }


    private Mesh LoadAssimpMesh(double scale, Assimp.Mesh assimpMesh)
    {
        Mesh engineMesh = new();
        engineMesh.Name = assimpMesh.Name;
        int vertexCount = assimpMesh.VertexCount;
        engineMesh.IndexFormat = vertexCount >= ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;

        Vector3[] vertices = LoadAssimpMeshVertices(scale, vertexCount, assimpMesh, engineMesh);

        if (assimpMesh.HasNormals)
        {
            LoadAssimpMeshNormals(vertexCount, assimpMesh, engineMesh);
        }
        else
        {
            Application.Logger.Warn($"Mesh '{assimpMesh.Name}' has no normals, recalculating...");
            engineMesh.RecalculateNormals();
        }

        if (assimpMesh.HasTangentBasis)
        {
            LoadAssimpMeshTangents(vertexCount, assimpMesh, engineMesh);
        }
        else
        {
            Application.Logger.Warn($"Mesh '{assimpMesh.Name}' has no tangents, recalculating...");
            engineMesh.RecalculateTangents();
        }

        if (assimpMesh.HasTextureCoords(0))
            LoadAssimpMeshTexCoords(vertexCount, assimpMesh, 0, engineMesh);

        if (assimpMesh.HasTextureCoords(1))
            LoadAssimpMeshTexCoords(vertexCount, assimpMesh, 1, engineMesh);

        if (assimpMesh.HasVertexColors(0))
            LoadAssimpMeshVertexColors(vertexCount, assimpMesh, engineMesh);

        engineMesh.SetIndices(assimpMesh.GetIndices());

        engineMesh.RecalculateBounds();

        if (assimpMesh.HasBones)
            LoadAssimpMeshBones(scale, engineMesh, assimpMesh, vertexCount, vertices);
        return engineMesh;
    }


    private static Vector3[] LoadAssimpMeshVertices(double scale, int vertexCount, Assimp.Mesh assimpMesh, Mesh engineMesh)
    {
        Vector3[] vertices = new Vector3[vertexCount];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = new Vector3(assimpMesh.Vertices[i].X, assimpMesh.Vertices[i].Y, assimpMesh.Vertices[i].Z) * (float)scale;
        engineMesh.SetVertexPositions(vertices);
        return vertices;
    }


    private void LoadAssimpMeshNormals(int vertexCount, Assimp.Mesh assimpMesh, Mesh engineMesh)
    {
        Vector3[] normals = new Vector3[vertexCount];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = new Vector3(assimpMesh.Normals[i].X, assimpMesh.Normals[i].Y, assimpMesh.Normals[i].Z);
            if (InvertNormals)
                normals[i] = -normals[i];
        }

        engineMesh.SetVertexNormals(normals);
    }


    private static void LoadAssimpMeshTangents(int vertexCount, Assimp.Mesh assimpMesh, Mesh engineMesh)
    {
        Vector3[] tangents = new Vector3[vertexCount];
        for (int i = 0; i < tangents.Length; i++)
            tangents[i] = new Vector3(assimpMesh.Tangents[i].X, assimpMesh.Tangents[i].Y, assimpMesh.Tangents[i].Z);
        engineMesh.SetVertexTangents(tangents);
    }


    private static void LoadAssimpMeshTexCoords(int vertexCount, Assimp.Mesh assimpMesh, int channelIndex, Mesh engineMesh)
    {
        Vector2[] texCoords = new Vector2[vertexCount];
        for (int i = 0; i < texCoords.Length; i++)
            texCoords[i] = new Vector2(assimpMesh.TextureCoordinateChannels[channelIndex][i].X, assimpMesh.TextureCoordinateChannels[channelIndex][i].Y);
        engineMesh.SetVertexUVs(texCoords, channelIndex);
    }


    private static void LoadAssimpMeshVertexColors(int vertexCount, Assimp.Mesh assimpMesh, Mesh engineMesh)
    {
        ColorRGBA[] colors = new ColorRGBA[vertexCount];
        for (int i = 0; i < colors.Length; i++)
        {
            byte r = (byte)(assimpMesh.VertexColorChannels[0][i].R * 255);
            byte g = (byte)(assimpMesh.VertexColorChannels[0][i].G * 255);
            byte b = (byte)(assimpMesh.VertexColorChannels[0][i].B * 255);
            byte a = (byte)(assimpMesh.VertexColorChannels[0][i].A * 255);
            colors[i] = new ColorRGBA(
                r, g, b, a);
        }

        engineMesh.SetVertexColors(colors);
    }
    
    
    private static void LoadAssimpMeshBones(double scale, Mesh engineMesh, Assimp.Mesh assimpMesh, int vertexCount, Vector3[] vertices)
    {
        engineMesh.BindPoses = new Matrix4x4[assimpMesh.Bones.Count];
        Vector4[] boneIndices = new Vector4[vertexCount];
        Vector4[] boneWeights = new Vector4[vertexCount];
        
        // Initialize bone weights, indices and bind poses
        for (int i = 0; i < assimpMesh.Bones.Count; i++)
            LoadAssimpMeshBone(scale, engineMesh, assimpMesh, i, boneIndices, boneWeights);

        // Normalize bone weights
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector4 w = boneWeights[i];
            float totalWeight = w.X + w.Y + w.Z + w.W;
                    
            if (Mathematics.MathOps.AlmostZero(totalWeight))
                continue;
                    
            w /= totalWeight;
                    
            boneWeights[i] = w;
        }
                
        engineMesh.SetBoneWeights(boneWeights);
        engineMesh.SetBoneIndices(boneIndices);
    }


    private static void LoadAssimpMeshBone(double scale, Mesh engineMesh, Assimp.Mesh assimpMesh, int i, Vector4[] boneIndices, Vector4[] boneWeights)
    {
        Bone? bone = assimpMesh.Bones[i];

        Assimp.Matrix4x4 offsetMatrix = bone.OffsetMatrix;
        Matrix4x4 bindPose = new(
            offsetMatrix.A1, offsetMatrix.B1, offsetMatrix.C1, offsetMatrix.D1,
            offsetMatrix.A2, offsetMatrix.B2, offsetMatrix.C2, offsetMatrix.D2,
            offsetMatrix.A3, offsetMatrix.B3, offsetMatrix.C3, offsetMatrix.D3,
            offsetMatrix.A4, offsetMatrix.B4, offsetMatrix.C4, offsetMatrix.D4
        );

        // Adjust translation by scale
        bindPose.SetTranslation(bindPose.Translation * (float)scale);

        engineMesh.BindPoses![i] = bindPose;

        if (!bone.HasVertexWeights)
            return;
            
        byte boneIndex = (byte)(i + 1);

        // foreach weight
        for (int j = 0; j < bone.VertexWeightCount; j++)
        {
            VertexWeight weight = bone.VertexWeights[j];
            Vector4 b = boneIndices[weight.VertexID];
            Vector4 w = boneWeights[weight.VertexID];
            if (b.X.AlmostZero() || weight.Weight > w.X)
            {
                b = b.SetX(boneIndex);
                w = w.SetX(weight.Weight);
            }
            else if (b.Y.AlmostZero() || weight.Weight > w.Y)
            {
                b = b.SetY(boneIndex);
                w = w.SetY(weight.Weight);
            }
            else if (b.Z.AlmostZero() || weight.Weight > w.Z)
            {
                b = b.SetZ(boneIndex);
                w = w.SetZ(weight.Weight);
            }
            else if (b.W.AlmostZero() || weight.Weight > w.W)
            {
                b = b.SetW(boneIndex);
                w = w.SetW(weight.Weight);
            }
            else
            {
                Application.Logger.Warn($"Vertex {weight.VertexID} has more than 4 bone weights, Skipping...");
            }

            boneIndices[weight.VertexID] = b;
            boneWeights[weight.VertexID] = w;
        }
    }


    private void AddMeshComponent(List<(Entity entity, Node node)> entityHierarchy, Entity entity, MeshMaterialBinding meshMaterialBinding)
    {
        if (meshMaterialBinding.AssimpMesh.HasBones)
        {
            // Add a skinned mesh renderer
            SkinnedMeshRenderer mr = entity.AddComponent<SkinnedMeshRenderer>();
            mr.Mesh = meshMaterialBinding.EngineMesh;
            mr.Material = meshMaterialBinding.EngineMaterial;
            
            // Find all bones in the hierarchy and assign them to the skinned mesh renderer
            List<Bone> assimpMeshBones = meshMaterialBinding.AssimpMesh.Bones;
            mr.Bones = new Transform[assimpMeshBones.Count];
            for (int i = 0; i < assimpMeshBones.Count; i++)
                mr.Bones[i] = entityHierarchy[0].entity.Transform.DeepFind(assimpMeshBones[i].Name)!.Entity.Transform;
        }
        else
        {
            // Add a mesh renderer
            MeshRenderer mr = entity.AddComponent<MeshRenderer>();
            mr.Mesh = meshMaterialBinding.EngineMesh;
            mr.Material = meshMaterialBinding.EngineMaterial;
        }

        if (GenerateColliders)
            throw new NotImplementedException();
    }

    #endregion


    private sealed class MeshMaterialBinding(string meshName, Assimp.Mesh assimpMesh, AssetRef<Mesh> engineMesh, AssetRef<Material> engineMaterial)
    {
        public string MeshName { get; } = meshName;
        public AssetRef<Material> EngineMaterial { get; } = engineMaterial;
        public AssetRef<Mesh> EngineMesh { get; } = engineMesh;
        public Assimp.Mesh AssimpMesh { get; } = assimpMesh;
    }
}