using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

public class MaterialUtility
{
    const string MAT_DIR_NAME = "Art/matVariant";
    static readonly StringBuilder s_StringBuilder = new StringBuilder();
    public static Material GetOrGenerateMaterialVariant(Shader shader, params object[] append)
    {
        if (!shader)
        {
            return null;
        }
        string[] keywords = append.Where(x => 0 < (int)x)
                .Select(x => x.ToString().ToUpper())
                .ToArray();

        Material mat = GetMaterial(shader, append);
        if (mat)
        {
            if (!mat.shaderKeywords.OrderBy(x => x).SequenceEqual(keywords.OrderBy(x => x)))
            {
                mat.shaderKeywords = keywords;
                EditorUtility.SetDirty(mat);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }
        string variantName = GetVariantName(shader, append);
        Debug.Log("Generate material : " + variantName);
        mat = new Material(shader);
        mat.shaderKeywords = keywords;
        mat.name = variantName;
        // mat.hideFlags |= HideFlags.NotEditable;
        SaveMaterial(mat, shader);
        return mat;
    }

    public static Material GetMaterial(Shader shader, params object[] append)
    {
        string variantName = GetVariantName(shader, append);
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(string.Format("{0}{1}.mat", GetMaterialDir(shader), variantName));
        return mat;
    }
    public static string GetVariantName(Shader shader, params object[] append)
    {
        s_StringBuilder.Length = 0;
        s_StringBuilder.Append(Path.GetFileName(shader.name));
        foreach (object mode in append.Where(x => 0 < (int)x))
        {
            s_StringBuilder.Append("-");
            s_StringBuilder.Append(mode.ToString());
        }
        return s_StringBuilder.ToString();
    }
    static void SaveMaterial(Material mat, Shader shader)
    {
        string matDir = GetMaterialDirFull(shader);
        Directory.CreateDirectory(matDir);
        AssetDatabase.CreateAsset(mat, string.Format("{0}/{1}.mat", GetMaterialDir(shader), mat.name));
        AssetDatabase.SaveAssets();
    }
    static string GetMaterialDirFull(Shader shader)
    {
        return string.Format("{0}/{1}/{2}/", Application.dataPath, MAT_DIR_NAME, Path.GetFileName(shader.name));
    }
    static string GetMaterialDir(Shader shader)
    {
        return string.Format("Assets/{0}/{1}/", MAT_DIR_NAME, Path.GetFileName(shader.name));
    }
}
