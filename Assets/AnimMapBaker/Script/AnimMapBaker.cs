/*
 * 用来烘焙动作贴图。烘焙对象使用animation组件，并且在导入时设置Rig为Legacy
 */
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AnimMapBaker
{
    /// <summary>
    /// 保存需要烘焙的动画的相关数据
    /// </summary>
    public struct AnimData
    {
        #region 字段

        public int vertexCount;
        public int mapWidth;
        public List<AnimationState> animClips;
        public string name;

        private  Animation animation;
        private SkinnedMeshRenderer skin;

        #endregion

        public AnimData(Animation anim, SkinnedMeshRenderer smr, string goName)
        {
            vertexCount = smr.sharedMesh.vertexCount;
            mapWidth = Mathf.NextPowerOfTwo(vertexCount);
            animClips = new List<AnimationState>(anim.Cast<AnimationState>());
            animation = anim;
            skin = smr;
            name = goName;
        }

        #region 方法

        public void AnimationPlay(string animName)
        {
            this.animation.Play(animName);
        }

        public void SampleAnimAndBakeMesh(ref Mesh m)
        {
            this.SampleAnim();
            this.BakeMesh(ref m);
        }

        private void SampleAnim()
        {
            if (this.animation == null)
            {
                Debug.LogError("animation is null!!");
                return;
            }

            this.animation.Sample();
        }

        private void BakeMesh(ref Mesh m)
        {
            if (this.skin == null)
            {
                Debug.LogError("skin is null!!");
                return;
            }

            this.skin.BakeMesh(m);
        }
        #endregion
    }

    /// <summary>
    /// 烘焙后的数据
    /// </summary>
    public struct BakedData
    {
        #region 字段

        public string name;
        public float animLen;
        public byte[] rawAnimMap;
        public int animMapWidth;
        public int animMapHeight;
        public Vector3 minPos;
        public Vector3 maxPos;

        #endregion

        public BakedData(string name, float animLen, Texture2D animMap, Vector3 minPos, Vector3 maxPos)
        {
            this.name = name;
            this.animLen = animLen;
            this.animMapHeight = animMap.height;
            this.animMapWidth = animMap.width;
            this.rawAnimMap = animMap.GetRawTextureData();
            this.maxPos = maxPos;
            this.minPos = minPos;
        }
    }

    /// <summary>
    /// 烘焙器
    /// </summary>
    public class AnimMapBaker{

        #region 字段

        private AnimData? animData = null;
        private List<Vector3> vertices = new List<Vector3>();
        private Mesh bakedMesh;

        private List<BakedData> bakedDataList = new List<BakedData>();

        #endregion

        #region 方法

        public void SetAnimData(GameObject go)
        {
            if(go == null)
            {
                Debug.LogError("go is null!!");
                return;
            }

            Animation anim = go.GetComponent<Animation>();
            SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

            if(anim == null || smr == null)
            {
                Debug.LogError("anim or smr is null!!");
                return;
            }
            this.bakedMesh = new Mesh();
            this.animData = new AnimData(anim, smr, go.name);
        }

        public List<BakedData> Bake()
        {
            if(this.animData == null)
            {
                Debug.LogError("bake data is null!!");
                return this.bakedDataList;
            }

            //每一个动作都生成一个动作图
            for(int i = 0; i < this.animData.Value.animClips.Count; i++)
            {
                if(!this.animData.Value.animClips[i].clip.legacy)
                {
                    Debug.LogError(string.Format("{0} is not legacy!!", this.animData.Value.animClips[i].clip.name));
                    continue;
                }
                BakePerAnimClip(this.animData.Value.animClips[i]);
            }

            return this.bakedDataList;
        }

        private void BakePerAnimClip(AnimationState curAnim)
        {
            int curClipFrame = 0;
            float sampleTime = 0;
            float perFrameTime = 0;

            //获取总帧数（帧率乘以秒数） 转换成2的幂
            curClipFrame = Mathf.ClosestPowerOfTwo((int)(curAnim.clip.frameRate * curAnim.length));
            //总秒数/总帧数 获得每帧的时间（s）
            perFrameTime = curAnim.length / curClipFrame;

            //mapWidth是顶点数 大的最小的2的幂
            Texture2D animMap = new Texture2D(this.animData.Value.mapWidth, curClipFrame, TextureFormat.RGB24, false);
            animMap.name = string.Format("{0}_{1}.animMap", this.animData.Value.name, curAnim.name);
            this.animData.Value.AnimationPlay(curAnim.name);
            Vector3 minPos = new Vector3(0, 0, 0);
            Vector3 maxPos = new Vector3(0, 0, 0);
            //所有帧，所有坐标，最大最小值
            for(int i = 0; i < curClipFrame; i++)
            {
                curAnim.time = sampleTime;
                this.animData.Value.SampleAnimAndBakeMesh(ref this.bakedMesh);
                for (int j = 0; j < this.bakedMesh.vertexCount; j++)
                {
                    Vector3 vertex = this.bakedMesh.vertices[j];
                    if (vertex.x > maxPos.x)
                        maxPos.x = vertex.x;
                    else if (vertex.x < minPos.x)
                        minPos.x = vertex.x;

                    if (vertex.y > maxPos.y)
                        maxPos.y = vertex.y;
                    else if (vertex.y < minPos.y)
                        minPos.y = vertex.y;

                    if (vertex.z > maxPos.z)
                        maxPos.z = vertex.z;
                    else if (vertex.z < minPos.z)
                        minPos.z = vertex.z;
                }
                sampleTime += perFrameTime;
            }
            sampleTime = 0;
            for (int i = 0; i < curClipFrame; i++)
            {
                curAnim.time = sampleTime;

                this.animData.Value.SampleAnimAndBakeMesh(ref this.bakedMesh);
                //x轴，顶点，y轴帧
                for (int j = 0; j < this.bakedMesh.vertexCount; j++)
                {
                    Vector3 vertex = this.bakedMesh.vertices[j];
                    Vector3 diff = new Vector3(0.0f, 0.0f, 0.0f);
                    diff.x = (vertex.x - minPos.x) / (maxPos.x - minPos.x);
                    diff.y = (vertex.y - minPos.y) / (maxPos.y - minPos.y);
                    diff.z = (vertex.z - minPos.z) / (maxPos.z - minPos.z);
                    animMap.SetPixel(j, i, new Color(diff.x, diff.y, diff.z));
                }
                sampleTime += perFrameTime;
            }
            animMap.Apply();

            this.bakedDataList.Add(new BakedData(animMap.name, curAnim.clip.length, animMap, minPos, maxPos));
        }
        #endregion
    }
        
}

