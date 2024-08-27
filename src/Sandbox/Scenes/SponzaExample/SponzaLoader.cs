using System.Collections;
using KorpiEngine.AssetManagement;
using KorpiEngine.AssetManagement.Importers;
using KorpiEngine.Core.Internal.AssetManagement.Importers;
using KorpiEngine.EntityModel;
using KorpiEngine.Networking;

namespace Sandbox.Scenes.SponzaExample;

/// <summary>
/// A component that loads the Sponza model from the web and spawns it in the scene.
/// </summary>
internal class SponzaLoader : EntityComponent
{
    private const string SPONZA_WEB_URL = "https://github.com/jimmiebergmann/Sponza/raw/master";
    private static readonly string[] SponzaAssets =
    [
        "sponza.obj",
        "sponza.mtl",
        "textures/background.tga",
        "textures/background_ddn.tga",
        "textures/chain_texture.tga",
        "textures/chain_texture_ddn.tga",
        "textures/lion.tga",
        "textures/lion2_ddn.tga",
        "textures/lion_ddn.tga",
        "textures/spnza_bricks_a_ddn.tga",
        "textures/spnza_bricks_a_diff.tga",
        "textures/sponza_arch_ddn.tga",
        "textures/sponza_arch_diff.tga",
        "textures/sponza_ceiling_a_ddn.tga",
        "textures/sponza_ceiling_a_diff.tga",
        "textures/sponza_column_a_ddn.tga",
        "textures/sponza_column_a_diff.tga",
        "textures/sponza_column_b_ddn.tga",
        "textures/sponza_column_b_diff.tga",
        "textures/sponza_column_c_ddn.tga",
        "textures/sponza_column_c_diff.tga",
        "textures/sponza_curtain_blue_diff.tga",
        "textures/sponza_curtain_ddn.tga",
        "textures/sponza_curtain_diff.tga",
        "textures/sponza_curtain_green_diff.tga",
        "textures/sponza_details_ddn.tga",
        "textures/sponza_details_diff.tga",
        "textures/sponza_fabric_blue_diff.tga",
        "textures/sponza_fabric_ddn.tga",
        "textures/sponza_fabric_diff.tga",
        "textures/sponza_fabric_green_diff.tga",
        "textures/sponza_flagpole_ddn.tga",
        "textures/sponza_flagpole_diff.tga",
        "textures/sponza_floor_a_ddn.tga",
        "textures/sponza_floor_a_diff.tga",
        "textures/sponza_roof_ddn.tga",
        "textures/sponza_roof_diff.tga",
        "textures/sponza_thorn_ddn.tga",
        "textures/sponza_thorn_diff.tga",
        "textures/vase_ddn.tga",
        "textures/vase_dif.tga",
        "textures/vase_hanging.tga",
        "textures/vase_hanging_ddn.tga",
        "textures/vase_plant.tga",
        "textures/vase_round.tga",
        "textures/vase_round_ddn.tga"
    ];
    
    
    protected override void OnStart()
    {
        StartCoroutine(LoadSponzaWeb());
    }
    
    
    private IEnumerator LoadSponzaWeb()
    {
        // Create a web request to load the Sponza model and all its assets,
        // and save them to disk next to the executable in "WebAssets/sponza" subfolder.
        WebAssetLoadOperation operation = new("sponza", SPONZA_WEB_URL, false, SponzaAssets);
        
        // Send the web request
        yield return operation.SendWebRequest();
        
        // Load the Sponza model from disk
        LoadSponzaDisk(operation.SavePaths[0]);
    }
    
    
    private void LoadSponzaDisk(string path)
    {
        // Create a custom importer to scale the model down
        ModelImporter importer = (ModelImporter)AssetDatabase.GetImporter(".obj");
        importer.UnitScale = 0.1f;
        
        // Load the Sponza model from disk
        Entity asset = AssetDatabase.LoadAssetFile<Entity>(path, importer);
        
        // Spawn the Sponza model in the scene
        asset.Spawn(Entity.Scene!);
    }
}