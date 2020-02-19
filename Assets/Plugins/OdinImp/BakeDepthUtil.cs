﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct BakeDepthParam
{
    public Vector2Int Size;
    public float UnitSize;
    public Shader DepthShader;
    public float Bottom;
    public float Top;
}

public class BakeDepthUtil
{
    #region var
    static BakeDepthUtil m_instance = new BakeDepthUtil();
    Camera m_cam;
    RenderTexture m_rt;
    BakeDepthParam m_param;
    #endregion

    #region  method
    public static RenderTexture BakeDepth(GameObject plane, in BakeDepthParam param)
    {
        return m_instance.InitParam(plane, in param);
    }

    RenderTexture InitParam(GameObject plane, in BakeDepthParam param)
    {
        //设置
        Init(param);

        //设置相机
        InitCamera(plane);
        //用相机渲染深度
        RenderDepth();
        return m_rt;
        //把深度图存起来
        //SaveDepthTexture();

        //后处理
        //CleanUp();
    }
    
    void Init(in BakeDepthParam param)
    {
        m_param = param;

        GameObject cam_obj = new GameObject("DepthRenderCam");
        cam_obj.AddComponent<Camera>();
        m_cam = cam_obj.GetComponent<Camera>();
        m_rt = RenderTexture.GetTemporary(m_param.Size.x, m_param.Size.y, 24);
    }

    void InitCamera(GameObject plane)
    {
        Graphics.SetRenderTarget(m_rt);
        //根据plane设置相机相关参数
        m_cam.targetTexture = m_rt;
        m_cam.orthographic = true;
        m_cam.nearClipPlane = 0.1f;
        m_cam.farClipPlane = 1000f;
        m_cam.transform.forward = new Vector3(0, -1, 0);
        float width = m_param.Size.x * m_param.UnitSize;
        float height = m_param.Size.y * m_param.UnitSize;
        m_cam.aspect = height / width;
        m_cam.enabled = true;
        m_cam.orthographicSize = width;
        m_cam.clearFlags = CameraClearFlags.SolidColor;
        m_cam.backgroundColor = Color.black;
        m_cam.gameObject.transform.position = plane.transform.position + new Vector3(0, 1, 0);
    }

    void RenderDepth()
    {
        Shader.SetGlobalFloat("_DepthRangeBottom", m_param.Bottom);
        Shader.SetGlobalFloat("_DepthRangeTop", m_param.Top);
        //RenderWithShader在URP中被弃用了，所以下面的做法不能达到效果
        //m_cam.RenderWithShader(m_param.DepthShader, "RenderType");
        //使用下面的方式进行替代
        ScriptableObject obj = AssetDatabase.LoadAssetAtPath(@"Assets/Settings/UniversalRP-LowQuality.asset", typeof(ScriptableObject)) as ScriptableObject;
        SerializedObject se_obj = new SerializedObject(obj);
        SerializedProperty pro = se_obj.FindProperty("m_DefaultRendererIndex");
        pro.intValue = 1;
        se_obj.ApplyModifiedProperties();
        m_cam.Render();
    }

    void SaveDepthTexture()
    {

    }

    public void CleanUp()
    {
        m_cam = null;
        RenderTexture.ReleaseTemporary(m_rt);
    }
    #endregion
}
