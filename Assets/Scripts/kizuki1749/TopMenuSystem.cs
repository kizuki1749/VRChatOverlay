using EasyLazyLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopMenuSystem : MonoBehaviour
{
    [SerializeField]
    private EasyOpenVROverlayForUnity EasyOpenVROverlay;
    [SerializeField]
    private GameObject[] Panels = new GameObject[2];

    EasyOpenVRUtil util = new EasyOpenVRUtil();

    void Start()
    {
        util.Init();
    }

    void Update()
    {
        if (!util.IsReady())
        {
            util.Init();
            return;
        }

        ulong button = 0;

        EasyOpenVRUtil.Transform pos = util.GetHMDTransform();
        EasyOpenVRUtil.Transform cpos = null;
        if (MenuSystemUtil.GetControllerHand(util) == ControllerHand.Right)
        {
            if (util.GetControllerButtonPressed(util.GetRightControllerIndex(), out button) && (PlayerPrefs.GetInt("NeedButtonPush", 1) == 0 || button == 0))
            {
                cpos = util.GetRightControllerTransform();
            }
        }
        else if (MenuSystemUtil.GetControllerHand(util) == ControllerHand.Left)
        {
            if (util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out button) && (PlayerPrefs.GetInt("NeedButtonPush", 1) == 0 || button == 0))
            {
                cpos = util.GetLeftControllerTransform();
            }
        }
    }

    public void ClearAllCanvas()
    {
        foreach (GameObject panel in Panels)
            panel.SetActive(false);
    }

    public void SetVisiblePanel(int index)
    {
        ClearAllCanvas();
        Panels[index].SetActive(true);
    }
}