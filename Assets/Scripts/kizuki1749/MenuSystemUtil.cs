using EasyLazyLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystemUtil
{
    public static ControllerHand GetControllerHand(EasyOpenVRUtil util)
    {
        ulong Leftbutton, Rightbutton;
        if (util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out Leftbutton))
        {
            if (Leftbutton != 0)
            {
                return ControllerHand.Left;
            }
        }
        if (util.GetControllerButtonPressed(util.GetRightControllerIndex(), out Rightbutton))
        {
            if (Rightbutton != 0)
            {
                return ControllerHand.Right;
            }
        }
        return ControllerHand.None;
    }

    public static ulong GetButton(EasyOpenVRUtil util, ControllerHand hand)
    {
        if (util == null)
        {
            util = new EasyOpenVRUtil();
            util.Init();
        }
        ulong Leftbutton, Rightbutton;
        if ((hand == ControllerHand.Left || hand == ControllerHand.Any) && util.GetControllerButtonPressed(util.GetLeftControllerIndex(), out Leftbutton))
        {
            if (hand == ControllerHand.Any && Leftbutton != 0)
                return Leftbutton;
            else if (hand == ControllerHand.Left)
                return Leftbutton;
        }
        if ((hand == ControllerHand.Right || hand == ControllerHand.Any) && util.GetControllerButtonPressed(util.GetRightControllerIndex(), out Rightbutton))
        {
            if (hand == ControllerHand.Any && Rightbutton != 0)
                return Rightbutton;
            else if (hand == ControllerHand.Right)
                return Rightbutton;
        }
        return 0;
    }
}

public enum ControllerHand
{
    None = 0, Left, Right, Any
}