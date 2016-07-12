﻿using System.Reflection;
using ColossalFramework;
using NetworkSkins.Detour;
using UnityEngine;
using System;
using System.IO;
using NetworkSkins.Data;
using NetworkSkins.Props;

namespace NetworkSkins.Skins
{
    public struct NetSegmentDetour // TODO detour
    {
        private static bool deployed = false;

        private static RedirectCallsState _NetSegment_RenderInstance_state;
        private static MethodInfo _NetSegment_RenderInstance_original;
        private static MethodInfo _NetSegment_RenderInstance_detour;

        private static RedirectCallsState _NetSegment_RenderLod_state;
        private static MethodInfo _NetSegment_RenderLod_original;
        private static MethodInfo _NetSegment_RenderLod_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _NetSegment_RenderInstance_original = typeof(NetSegment).GetMethod("RenderInstance", BindingFlags.Instance | BindingFlags.NonPublic);
                _NetSegment_RenderInstance_detour = typeof(NetSegmentDetour).GetMethod("RenderInstance", BindingFlags.Instance | BindingFlags.NonPublic);
                _NetSegment_RenderInstance_state = RedirectionHelper.RedirectCalls(_NetSegment_RenderInstance_original, _NetSegment_RenderInstance_detour);

                _NetSegment_RenderLod_original = typeof(NetSegment).GetMethod("RenderLod", BindingFlags.Static | BindingFlags.Public);
                _NetSegment_RenderLod_detour = typeof(NetSegmentDetour).GetMethod("RenderLod", BindingFlags.Static | BindingFlags.Public);
                _NetSegment_RenderLod_state = RedirectionHelper.RedirectCalls(_NetSegment_RenderLod_original, _NetSegment_RenderLod_detour);

                deployed = true;
            }

            // TEST CODE
            /*
            if (false)
            {
                var networkName = "Basic Road";

                var segmentMaterial = PrefabCollection<NetInfo>.FindLoaded(networkName).m_segments[0].m_segmentMaterial;
                Texture2D texture2D;
                if (File.Exists("tt/" + networkName + "_D.png"))
                {
                    texture2D = new Texture2D(1, 1);
                    texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_D.png"));
                    texture2D.anisoLevel = 0;
                    segmentMaterial.SetTexture("_MainTex", texture2D);
                }
                if (File.Exists("tt/" + networkName + "_APR.png"))
                {
                    texture2D = new Texture2D(1, 1);
                    texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_APR.png"));
                    texture2D.anisoLevel = 0;
                    segmentMaterial.SetTexture("_APRMap", texture2D);
                }
                if (File.Exists("tt/" + networkName + "_XYS.png"))
                {
                    texture2D = new Texture2D(1, 1);
                    texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_XYS.png"));
                    texture2D.anisoLevel = 0;
                    segmentMaterial.SetTexture("_XYSMap", texture2D);
                }
            }
            */
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_NetSegment_RenderInstance_original, _NetSegment_RenderInstance_state);
                _NetSegment_RenderInstance_original = null;
                _NetSegment_RenderInstance_detour = null;

                RedirectionHelper.RevertRedirect(_NetSegment_RenderLod_original, _NetSegment_RenderLod_state);
                _NetSegment_RenderLod_original = null;
                _NetSegment_RenderLod_detour = null;

                deployed = false;
            }
        }

        // NetSegment
        private void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, int layerMask, NetInfo info, ref RenderManager.Instance data)
        {
            

            // mod begin
            var _this = NetManager.instance.m_segments.m_buffer[segmentID];
            var skin = SegmentDataManager.Instance.SegmentToSegmentDataMap?[segmentID]?.SkinPrefab;
            // mod end

            NetManager instance = Singleton<NetManager>.instance;
            if (data.m_dirty)
            {
                data.m_dirty = false;
                Vector3 position = instance.m_nodes.m_buffer[(int)_this.m_startNode].m_position;
                Vector3 position2 = instance.m_nodes.m_buffer[(int)_this.m_endNode].m_position;
                data.m_position = (position + position2) * 0.5f;
                data.m_rotation = Quaternion.identity;
                data.m_dataColor0 = info.m_color;
                data.m_dataColor0.a = 0f;
                data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                Vector4 colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentID));
                Vector4 vector = colorLocation;
                if (NetNode.BlendJunction(_this.m_startNode))
                {
                    colorLocation = RenderManager.GetColorLocation(86016u + (uint)_this.m_startNode);
                }
                if (NetNode.BlendJunction(_this.m_endNode))
                {
                    vector = RenderManager.GetColorLocation(86016u + (uint)_this.m_endNode);
                }
                data.m_dataVector3 = new Vector4(colorLocation.x, colorLocation.y, vector.x, vector.y);
                if (info.m_segments == null || info.m_segments.Length == 0)
                {
                    if (info.m_lanes != null)
                    {
                        bool invert;
                        if ((_this.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                        {
                            invert = true;
                            NetInfo info2 = instance.m_nodes.m_buffer[(int)_this.m_endNode].Info;
                            NetNode.Flags flags;
                            Color color;
                            info2.m_netAI.GetNodeState(_this.m_endNode, ref instance.m_nodes.m_buffer[(int)_this.m_endNode], segmentID, ref _this, out flags, out color);
                            NetInfo info3 = instance.m_nodes.m_buffer[(int)_this.m_startNode].Info;
                            NetNode.Flags flags2;
                            Color color2;
                            info3.m_netAI.GetNodeState(_this.m_startNode, ref instance.m_nodes.m_buffer[(int)_this.m_startNode], segmentID, ref _this, out flags2, out color2);
                        }
                        else
                        {
                            invert = false;
                            NetInfo info4 = instance.m_nodes.m_buffer[(int)_this.m_startNode].Info;
                            NetNode.Flags flags;
                            Color color;
                            info4.m_netAI.GetNodeState(_this.m_startNode, ref instance.m_nodes.m_buffer[(int)_this.m_startNode], segmentID, ref _this, out flags, out color);
                            NetInfo info5 = instance.m_nodes.m_buffer[(int)_this.m_endNode].Info;
                            NetNode.Flags flags2;
                            Color color2;
                            info5.m_netAI.GetNodeState(_this.m_endNode, ref instance.m_nodes.m_buffer[(int)_this.m_endNode], segmentID, ref _this, out flags2, out color2);
                        }
                        float startAngle = (float)_this.m_cornerAngleStart * 0.0245436933f;
                        float endAngle = (float)_this.m_cornerAngleEnd * 0.0245436933f;
                        int num = 0;
                        uint num2 = _this.m_lanes;
                        int num3 = 0;
                        while (num3 < info.m_lanes.Length && num2 != 0u)
                        {
                            instance.m_lanes.m_buffer[(int)((UIntPtr)num2)].RefreshInstance(num2, info.m_lanes[num3], startAngle, endAngle, invert, ref data, ref num);
                            num2 = instance.m_lanes.m_buffer[(int)((UIntPtr)num2)].m_nextLane;
                            num3++;
                        }
                    }
                }
                else
                {
                    float vScale = info.m_netAI.GetVScale();
                    Vector3 vector2;
                    Vector3 startDir;
                    bool smoothStart;
                    _this.CalculateCorner(segmentID, true, true, true, out vector2, out startDir, out smoothStart);
                    Vector3 vector3;
                    Vector3 endDir;
                    bool smoothEnd;
                    _this.CalculateCorner(segmentID, true, false, true, out vector3, out endDir, out smoothEnd);
                    Vector3 vector4;
                    Vector3 startDir2;
                    _this.CalculateCorner(segmentID, true, true, false, out vector4, out startDir2, out smoothStart);
                    Vector3 vector5;
                    Vector3 endDir2;
                    _this.CalculateCorner(segmentID, true, false, false, out vector5, out endDir2, out smoothEnd);
                    Vector3 vector6;
                    Vector3 vector7;
                    NetSegment.CalculateMiddlePoints(vector2, startDir, vector5, endDir2, smoothStart, smoothEnd, out vector6, out vector7);
                    Vector3 vector8;
                    Vector3 vector9;
                    NetSegment.CalculateMiddlePoints(vector4, startDir2, vector3, endDir, smoothStart, smoothEnd, out vector8, out vector9);
                    data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(vector2, vector6, vector7, vector5, vector4, vector8, vector9, vector3, data.m_position, vScale);
                    data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(vector4, vector8, vector9, vector3, vector2, vector6, vector7, vector5, data.m_position, vScale);
                }
                if (info.m_requireSurfaceMaps)
                {
                    Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector1);
                }
            }
            if (info.m_segments != null)
            {
                for (int i = 0; i < info.m_segments.Length; i++)
                {
                    NetInfo.Segment segment = info.m_segments[i];
                    bool flag;
                    if (segment.CheckFlags(_this.m_flags, out flag))
                    {
                        Vector4 dataVector = data.m_dataVector3;
                        Vector4 dataVector2 = data.m_dataVector0;
                        if (segment.m_requireWindSpeed)
                        {
                            dataVector.w = data.m_dataFloat0;
                        }
                        if (flag)
                        {
                            dataVector2.x = -dataVector2.x;
                            dataVector2.y = -dataVector2.y;
                        }
                        if (cameraInfo.CheckRenderDistance(data.m_position, segment.m_lodRenderDistance))
                        {
                            instance.m_materialBlock.Clear();
                            instance.m_materialBlock.AddMatrix(instance.ID_LeftMatrix, data.m_dataMatrix0);
                            instance.m_materialBlock.AddMatrix(instance.ID_RightMatrix, data.m_dataMatrix1);
                            instance.m_materialBlock.AddVector(instance.ID_MeshScale, dataVector2);
                            instance.m_materialBlock.AddVector(instance.ID_ObjectIndex, dataVector);
                            instance.m_materialBlock.AddColor(instance.ID_Color, data.m_dataColor0);
                            if (segment.m_requireSurfaceMaps && data.m_dataTexture0 != null)
                            {
                                instance.m_materialBlock.AddTexture(instance.ID_SurfaceTexA, data.m_dataTexture0);
                                instance.m_materialBlock.AddTexture(instance.ID_SurfaceTexB, data.m_dataTexture1);
                                instance.m_materialBlock.AddVector(instance.ID_SurfaceMapping, data.m_dataVector1);
                            }
                            NetManager expr_5D7_cp_0 = instance;
                            expr_5D7_cp_0.m_drawCallData.m_defaultCalls = expr_5D7_cp_0.m_drawCallData.m_defaultCalls + 1;

                            if(skin == null) Graphics.DrawMesh(segment.m_segmentMesh, data.m_position, data.m_rotation, segment.m_segmentMaterial, segment.m_layer, null, 0, instance.m_materialBlock); // TODO
                            else if(skin.SegmentMaterials[i] != null) Graphics.DrawMesh(segment.m_segmentMesh, data.m_position, data.m_rotation, skin.SegmentMaterials[i], segment.m_layer, null, 0, instance.m_materialBlock); // TODO
                        }
                        else
                        {
                            NetInfo.LodValue combinedLod = segment.m_combinedLod;
                            if (combinedLod != null)
                            {
                                if (segment.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod.m_surfaceTexA)
                                {
                                    if (combinedLod.m_lodCount != 0)
                                    {
                                        NetSegment.RenderLod(cameraInfo, combinedLod);
                                    }
                                    combinedLod.m_surfaceTexA = data.m_dataTexture0;
                                    combinedLod.m_surfaceTexB = data.m_dataTexture1;
                                    combinedLod.m_surfaceMapping = data.m_dataVector1;
                                }
                                combinedLod.m_leftMatrices[combinedLod.m_lodCount] = data.m_dataMatrix0;
                                combinedLod.m_rightMatrices[combinedLod.m_lodCount] = data.m_dataMatrix1;
                                combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector2;
                                combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector;
                                combinedLod.m_meshLocations[combinedLod.m_lodCount] = data.m_position;
                                combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, data.m_position);
                                combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, data.m_position);
                                if (++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length)
                                {
                                    NetSegment.RenderLod(cameraInfo, combinedLod);
                                }
                            }
                        }
                    }
                }
            }
            if (info.m_lanes != null && ((layerMask & info.m_treeLayers) != 0 || cameraInfo.CheckRenderDistance(data.m_position, info.m_maxPropDistance + 128f)))
            {
                bool invert2;
                NetNode.Flags startFlags;
                Color startColor;
                NetNode.Flags endFlags;
                Color endColor;
                if ((_this.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                {
                    invert2 = true;
                    NetInfo info6 = instance.m_nodes.m_buffer[(int)_this.m_endNode].Info;
                    info6.m_netAI.GetNodeState(_this.m_endNode, ref instance.m_nodes.m_buffer[(int)_this.m_endNode], segmentID, ref _this, out startFlags, out startColor);
                    NetInfo info7 = instance.m_nodes.m_buffer[(int)_this.m_startNode].Info;
                    info7.m_netAI.GetNodeState(_this.m_startNode, ref instance.m_nodes.m_buffer[(int)_this.m_startNode], segmentID, ref _this, out endFlags, out endColor);
                }
                else
                {
                    invert2 = false;
                    NetInfo info8 = instance.m_nodes.m_buffer[(int)_this.m_startNode].Info;
                    info8.m_netAI.GetNodeState(_this.m_startNode, ref instance.m_nodes.m_buffer[(int)_this.m_startNode], segmentID, ref _this, out startFlags, out startColor);
                    NetInfo info9 = instance.m_nodes.m_buffer[(int)_this.m_endNode].Info;
                    info9.m_netAI.GetNodeState(_this.m_endNode, ref instance.m_nodes.m_buffer[(int)_this.m_endNode], segmentID, ref _this, out endFlags, out endColor);
                }
                float startAngle2 = (float)_this.m_cornerAngleStart * 0.0245436933f;
                float endAngle2 = (float)_this.m_cornerAngleEnd * 0.0245436933f;
                Vector4 objectIndex = new Vector4(data.m_dataVector3.x, data.m_dataVector3.y, 1f, data.m_dataFloat0);
                Vector4 objectIndex2 = new Vector4(data.m_dataVector3.z, data.m_dataVector3.w, 1f, data.m_dataFloat0);
                InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
                if (currentMode != InfoManager.InfoMode.None && !info.m_netAI.ColorizeProps(currentMode))
                {
                    objectIndex.z = 0f;
                    objectIndex2.z = 0f;
                }
                int num4 = (info.m_segments != null && info.m_segments.Length != 0) ? -1 : 0;
                uint num5 = _this.m_lanes;
                int num6 = 0;
                while (num6 < info.m_lanes.Length && num5 != 0u)
                {
                    instance.m_lanes.m_buffer[(int)((UIntPtr)num5)].RenderInstance(cameraInfo, segmentID, num5, info.m_lanes[num6], startFlags, endFlags, startColor, endColor, startAngle2, endAngle2, invert2, layerMask, objectIndex, objectIndex2, ref data, ref num4);
                    num5 = instance.m_lanes.m_buffer[(int)((UIntPtr)num5)].m_nextLane;
                    num6++;
                }
            }
        }
        // NetSegment
        public static void RenderLod(RenderManager.CameraInfo cameraInfo, NetInfo.LodValue lod)
        {
            NetManager instance = Singleton<NetManager>.instance;
            MaterialPropertyBlock materialBlock = instance.m_materialBlock;
            materialBlock.Clear();
            for (int i = 0; i < lod.m_lodCount; i++)
            {
                materialBlock.AddMatrix(instance.ID_LeftMatrices[i], lod.m_leftMatrices[i]);
                materialBlock.AddMatrix(instance.ID_RightMatrices[i], lod.m_rightMatrices[i]);
                materialBlock.AddVector(instance.ID_MeshScales[i], lod.m_meshScales[i]);
                materialBlock.AddVector(instance.ID_ObjectIndices[i], lod.m_objectIndices[i]);
                materialBlock.AddVector(instance.ID_MeshLocations[i], lod.m_meshLocations[i]);
            }
            Mesh mesh;
            int num;
            if (lod.m_lodCount <= 1)
            {
                mesh = lod.m_key.m_mesh.m_mesh1;
                num = 1;
            }
            else if (lod.m_lodCount <= 4)
            {
                mesh = lod.m_key.m_mesh.m_mesh4;
                num = 4;
            }
            else
            {
                mesh = lod.m_key.m_mesh.m_mesh8;
                num = 8;
            }
            for (int j = lod.m_lodCount; j < num; j++)
            {
                materialBlock.AddMatrix(instance.ID_LeftMatrices[j], default(Matrix4x4));
                materialBlock.AddMatrix(instance.ID_RightMatrices[j], default(Matrix4x4));
                materialBlock.AddVector(instance.ID_MeshScales[j], Vector4.zero);
                materialBlock.AddVector(instance.ID_ObjectIndices[j], Vector4.zero);
                materialBlock.AddVector(instance.ID_MeshLocations[j], cameraInfo.m_forward * -100000f);
            }
            if (lod.m_surfaceTexA != null)
            {
                materialBlock.AddTexture(instance.ID_SurfaceTexA, lod.m_surfaceTexA);
                materialBlock.AddTexture(instance.ID_SurfaceTexB, lod.m_surfaceTexB);
                materialBlock.AddVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_surfaceTexA = null;
                lod.m_surfaceTexB = null;
            }
            if (mesh != null)
            {
                Bounds bounds = default(Bounds);
                bounds.SetMinMax(lod.m_lodMin - new Vector3(100f, 100f, 100f), lod.m_lodMax + new Vector3(100f, 100f, 100f));
                mesh.bounds = bounds;
                lod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                lod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                NetManager expr_2AD_cp_0 = instance;
                expr_2AD_cp_0.m_drawCallData.m_lodCalls = expr_2AD_cp_0.m_drawCallData.m_lodCalls + 1;
                NetManager expr_2C0_cp_0 = instance;
                expr_2C0_cp_0.m_drawCallData.m_batchedCalls = expr_2C0_cp_0.m_drawCallData.m_batchedCalls + (lod.m_lodCount - 1);
                Graphics.DrawMesh(mesh, Matrix4x4.identity, lod.m_material, lod.m_key.m_layer, null, 0, materialBlock);
            }
            lod.m_lodCount = 0;
        }


    }
}
