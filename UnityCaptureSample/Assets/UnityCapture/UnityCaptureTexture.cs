/*
  UnityCaptureTexture.cs
  Copyright (c) 2018 Bernhard Schelling & Brandon J Matthews

  Unity Capture
  Copyright (c) 2018 Bernhard Schelling

  Based on UnityCam
  https://github.com/mrayy/UnityCam
  Copyright (c) 2016 MHD Yamen Saraiji

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/

using UnityEngine;

public class UnityCaptureTexture : MonoBehaviour
{

    [SerializeField] [Tooltip("Capture device index")] public UnityCapture.ECaptureDevice CaptureDevice = UnityCapture.ECaptureDevice.CaptureDevice1;
    [SerializeField] [Tooltip("Scale image if Unity and capture resolution don't match (can introduce frame dropping, not recommended)")] public UnityCapture.EResizeMode ResizeMode = UnityCapture.EResizeMode.Disabled;
    [SerializeField] [Tooltip("Mirror captured output image")] public UnityCapture.EMirrorMode MirrorMode = UnityCapture.EMirrorMode.Disabled;
    [SerializeField] [Tooltip("Introduce a frame of latency in favor of frame rate")] public bool DoubleBuffering = false;
    [SerializeField] [Tooltip("Check to enable VSync during capturing")] public bool EnableVSync = false;
    [SerializeField] [Tooltip("Set the desired render target frame rate")] public int TargetFrameRate = 60;
    [SerializeField] [Tooltip("Check to disable output of warnings")] public bool HideWarnings = false;
    private RenderTexture outputTexture;

    [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static System.IntPtr CaptureCreateInstance(int CapNum);
    [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static void CaptureDeleteInstance(System.IntPtr instance);
    [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static UnityCapture.ECaptureSendResult CaptureSendTexture(System.IntPtr instance, System.IntPtr nativetexture, bool UseDoubleBuffering, EResizeMode ResizeMode, EMirrorMode MirrorMode, bool IsLinearColorSpace);
    System.IntPtr CaptureInstance;

    void Awake()
    {
        QualitySettings.vSyncCount = (EnableVSync ? 1 : 0);
        Application.targetFrameRate = TargetFrameRate;

        if (Application.runInBackground == false)
        {
            Debug.LogWarning("Application.runInBackground switched to enabled for capture streaming");
            Application.runInBackground = true;
        }
    }

    void Start()
    {
        CaptureInstance = CaptureCreateInstance((int)CaptureDevice);
    }

    void OnDestroy()
    {
        CaptureDeleteInstance(CaptureInstance);
    }

    public void UpdateTexture(Texture2D texture)
    {
        if (texture == null) return;
        if (outputTexture == null) outputTexture = new RenderTexture(texture.width, texture.height, 0);

        Graphics.Blit(texture, outputTexture);

        if (CaptureInstance == System.IntPtr.Zero) return;
        switch (CaptureSendTexture(CaptureInstance, outputTexture.GetNativeTexturePtr(), DoubleBuffering, ResizeMode, MirrorMode, QualitySettings.activeColorSpace == ColorSpace.Linear))
        {
            case UnityCapture.ECaptureSendResult.SUCCESS: break;
            case UnityCapture.ECaptureSendResult.WARNING_FRAMESKIP:               if (!HideWarnings) Debug.LogWarning("[UnityCapture] Capture device did skip a frame read, capture frame rate will not match render frame rate."); break;
            case UnityCapture.ECaptureSendResult.WARNING_CAPTUREINACTIVE:         if (!HideWarnings) Debug.LogWarning("[UnityCapture] Capture device is inactive"); break;
            case UnityCapture.ECaptureSendResult.ERROR_UNSUPPORTEDGRAPHICSDEVICE: Debug.LogError("[UnityCapture] Unsupported graphics device (only D3D11 supported)"); break;
            case UnityCapture.ECaptureSendResult.ERROR_PARAMETER:                 Debug.LogError("[UnityCapture] Input parameter error"); break;
            case UnityCapture.ECaptureSendResult.ERROR_TOOLARGERESOLUTION:        Debug.LogError("[UnityCapture] Render resolution is too large to send to capture device"); break;
            case UnityCapture.ECaptureSendResult.ERROR_TEXTUREFORMAT:             Debug.LogError("[UnityCapture] Render texture format is unsupported (only basic non-HDR (ARGB32) and HDR (FP16/ARGB Half) formats are supported)"); break;
            case UnityCapture.ECaptureSendResult.ERROR_READTEXTURE:               Debug.LogError("[UnityCapture] Error while reading texture image data"); break;
        }
    }
}
