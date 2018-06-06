using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AnimMapBaker
{
    public class AnimMapBakerWindow : EditorWindow
    {
        private enum SaveStrategy
        {
            AnimMap,      //only anim map
            Mat,          //with mat
            Prefab        //prefab with mat
        }

        #region 字段

        public static GameObject targetGo;
        public static Material targetMat;
        private static AnimMapBaker baker;
        private static string path = "CapstonesRes/Game/Models/Scene/Flag";
        private static string subPath = "";
        private static SaveStrategy stratege = SaveStrategy.Prefab;
        private static Shader animMapShader;

        #endregion

        #region  方法

        [MenuItem("3DArt/AnimMapBaker")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AnimMapBakerWindow));
            baker = new AnimMapBaker();
            animMapShader = Shader.Find("capstone/AnimMapShader");
        }

        void OnGUI()
        {
            targetGo = (GameObject)EditorGUILayout.ObjectField(targetGo, typeof(GameObject), true);
            subPath = targetGo == null ? subPath : targetGo.name;
            EditorGUILayout.LabelField(string.Format("保存路径output path:{0}", Path.Combine(path, subPath)));
            //获得一个长300的框  
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(400));
            //将上面的框作为文本输入框  
            path = EditorGUI.TextField(rect, path);
            subPath = EditorGUILayout.TextField(subPath);

            //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
            if ((Event.current.type == EventType.DragUpdated
              || Event.current.type == EventType.DragExited)
              && rect.Contains(Event.current.mousePosition))
            {
                //改变鼠标的外表  
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    path = DragAndDrop.paths[0];
                }
            }

            stratege = (SaveStrategy)EditorGUILayout.EnumPopup("保存策略output type:", stratege);
            if(stratege == SaveStrategy.Prefab || stratege == SaveStrategy.Mat)
            {
                targetMat = (Material)EditorGUILayout.ObjectField(targetMat, typeof(Material), true);
            }

            if (GUILayout.Button("Bake"))
            {
                if (targetGo == null)
                {
                    EditorUtility.DisplayDialog("err", "targetGo is null, 请填第一个空！", "OK");
                    return;
                }

                if (baker == null)
                {
                    baker = new AnimMapBaker();
                }

                baker.SetAnimData(targetGo);

                List<BakedData> list = baker.Bake();

                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        BakedData data = list[i];
                        Save(ref data);
                    }
                }
            }
        }

        private void Save(ref BakedData data)
        {
            switch (stratege)
            {
                case SaveStrategy.AnimMap:
                    SaveAsAsset(ref data);
                    break;
                case SaveStrategy.Mat:
                    SaveAsMat(ref data);
                    break;
                case SaveStrategy.Prefab:
                    SaveAsPrefab(ref data);
                    break;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private Texture2D SaveAsAsset(ref BakedData data)
        {
            string folderPath = CreateFolder();
            Texture2D animMap = new Texture2D(data.animMapWidth, data.animMapHeight, TextureFormat.RGB24, false);

            animMap.LoadRawTextureData(data.rawAnimMap);
            AssetDatabase.CreateAsset(animMap, Path.Combine(folderPath, data.name + ".asset"));
            return animMap;
        }

        private Material SaveAsMat(ref BakedData data)
        {
            if (targetGo == null || !targetGo.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                EditorUtility.DisplayDialog("err", "SkinnedMeshRender is null!!", "OK");
                return null;
            }

            SkinnedMeshRenderer smr = targetGo.GetComponentInChildren<SkinnedMeshRenderer>();
            Texture2D animMap = SaveAsAsset(ref data);
            Material mat = new Material(targetMat);
            mat.SetTexture("_AnimMap", animMap);
            mat.SetFloat("_AnimLen", data.animLen);
            Vector4 minPos = new Vector4(data.minPos.x, data.minPos.y, data.minPos.z, 0.0f);
            Vector4 maxPos = new Vector4(data.maxPos.x, data.maxPos.y, data.maxPos.z, 0.0f);
            mat.SetVector("_MinPos", minPos);
            mat.SetVector("_MaxPos", maxPos);

            string folderPath = CreateFolder();
            AssetDatabase.CreateAsset(mat, Path.Combine(folderPath, data.name + ".mat"));

            return mat;
        }

        private void SaveAsPrefab(ref BakedData data)
        {
            Material mat = SaveAsMat(ref data);

            if (mat == null)
            {
                EditorUtility.DisplayDialog("err", "mat is null!!", "OK");
                return;
            }

            GameObject go = new GameObject();
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.AddComponent<MeshFilter>().sharedMesh = targetGo.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            string folderPath = CreateFolder();
            PrefabUtility.CreatePrefab(Path.Combine(folderPath, data.name + ".prefab").Replace("\\", "/"), go);
        }

        private string CreateFolder()
        {
            string folderPath = Path.Combine(path, subPath);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(path, subPath);
            }
            return folderPath;
        }

        #endregion
    }
}
