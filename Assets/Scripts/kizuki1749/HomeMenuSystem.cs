using EasyLazyLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenuSystem : MonoBehaviour
{
    [SerializeField]
    private EasyOpenVROverlayForUnity EasyOpenVROverlay;
    [SerializeField]
    private Text TimeText;
    [SerializeField]
    private Canvas Canvas;

    public bool DisableChangeVisible = false;
    private EasyOpenVRUtil util = new EasyOpenVRUtil();
    public Vector3 OverlayPosition = new Vector3(0.03f, -0.25f, 0.5f); //HMDの前方50cm、25cm下の位置に表示
    public Vector3 OverlayRotation = new Vector3(-20f, 0, 0); //操作しやすいよう-20°傾ける

    private void Start()
    {
        util.Init();
    }

    // Update is called once per frame
    void Update()
    {
        TimeText.text = DateTime.Now.ToString("yyyy/MM/dd(ddd)\nHH:mm:ss");

        if (!DisableChangeVisible)
        {
            ulong key = ulong.Parse(PlayerPrefs.GetString("ToggleButton", "2"));
            if (MenuSystemUtil.GetButton(util, ControllerHand.Left) == key)
            {
                if (MenuSystemUtil.GetButton(util, ControllerHand.Right) == key)
                {
                   setPosition();
                    Canvas.gameObject.SetActive(!Canvas.gameObject.activeSelf);
                    DisableChangeVisible = true;
                    Invoke("DisableChangeVisibleSetFalse", 0.5F);
                }
            }
        }
    }

    //HMDの位置を基準に操作しやすい位置に画面を出す
    public void setPosition()
    {
        //HMDの姿勢情報を取得する
        var pos = util.GetHMDTransform();

        //HMDの姿勢情報が無効な場合は
        if (pos == null)
        {
            return; //更新しない
        }

        //HMDの位置に、基準位置とHMD角度を加算したものを、表示位置とする(でないと明後日の方向に移動するため)
        EasyOpenVROverlay.Position = pos.position + pos.rotation * OverlayPosition;

        //HMDの角度を一部反転したものに、基準角度を加算したものを、表示角度とする
        EasyOpenVROverlay.Rotation = (new Vector3(-pos.rotation.eulerAngles.x, -pos.rotation.eulerAngles.y, 0)) + OverlayRotation;

    }

    void DisableChangeVisibleSetFalse()
    {
        DisableChangeVisible = false;
    }
}
