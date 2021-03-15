using EasyLazyLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    private Text StatusText;
    [SerializeField]
    private Text NeedButtonPush;
    [SerializeField]
    private Text NeedButtonPushToMove;
    [SerializeField]
    private Text ToggleButtonSet;
    [SerializeField]
    private Text ControlButton;
    [SerializeField]
    private HomeMenuSystem homeMenuSystem;
    [SerializeField]
    private Canvas Canvas;
    [SerializeField]
    private DialogManager DialogManager;
    [SerializeField]
    private VRChat_Main VRChat;

    EasyOpenVRUtil util = new EasyOpenVRUtil();

    public void Start()
    {
        util.Init();
        Reload();
    }

    public void Reload()
    {
        NeedButtonPush.text = PlayerPrefs.GetInt("NeedButtonPush", 1) == 1 ? "コントローラーのボタンで操作する" : "画面をタッチして操作する";
        NeedButtonPushToMove.text = PlayerPrefs.GetInt("NeedButtonPushToMove", 1) == 1 ? "コントローラーのボタンを押しながら移動する\n(変更不可)" : "コントローラーのボタンを押さずに移動する\n(変更不可)";
        ToggleButtonSet.text = "表示非表示ボタンの変更 (現在値: " + PlayerPrefs.GetString("ToggleButton", "2") + ")\n両手で設定したボタンを押すと呼び出せます";
        ControlButton.text = "操作ボタンの変更 (現在値: " + PlayerPrefs.GetString("ControlButton", "4") + ")";
    }

    public void ChangeSettingsBool(string key)
    {
        bool val = PlayerPrefs.GetInt(key, 0) == 1;
        PlayerPrefs.SetInt(key, (!val) ? 1 : 0);
        SaveSettings();
        Reload();
    }

    public void ChangeNeedButtonPush()
    {
        bool val = PlayerPrefs.GetInt("NeedButtonPush", 1) == 1;
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            if (DialogManager.Result == 1)
                PlayerPrefs.SetInt("NeedButtonPush", 1);
            else
                PlayerPrefs.SetInt("NeedButtonPush", 0);
            SaveSettings();
            Reload();
        });
        DialogManager.ShowDialogSelect(unityEvent, "メニューの操作設定を変更することができます。\n・「ボタンで操作」\n操作ボタンでメニュー項目を操作します。\n・「タッチで操作」ボタンは押さずにコントローラーでタッチするとメニューを操作できます。\n\n移動ボタンについてはこの設定に関係なくトリガーまたは操作ボタンを引きながらの操作になります。", "操作設定", "ボタンで操作", "タッチで操作");
    }

    public async void GetKeyToStoreSetting()
    {
        homeMenuSystem.DisableChangeVisible = true;
        DialogManager.ShowDialogNoButton(null, "コントローラーを画面外に向けた状態でボタンを押してください。", "設定するボタンを押してください");
        homeMenuSystem.DisableChangeVisible = true;
        await Task.Delay(500);
        while (true)
        {
            ulong button = MenuSystemUtil.GetButton(util, ControllerHand.Any);
            if (button != 0)
            {
                PlayerPrefs.SetString("ToggleButton", button.ToString());
                break;
            }
            await Task.Delay(10);
        }
        SaveSettings();
        Reload();
        _ = DialogManager.NoButtonWindowClose();
        homeMenuSystem.DisableChangeVisible = false;
    }

    public async void GetKeyToStoreControlSetting()
    {
        DialogManager.ShowDialogNoButton(null, "コントローラーを画面外に向けた状態でボタンを押してください。", "設定するボタンを押してください");
        homeMenuSystem.DisableChangeVisible = true;
        await Task.Delay(500);
        while (true)
        {
            ulong button = MenuSystemUtil.GetButton(util, ControllerHand.Any);
            if (button != 0)
            {
                PlayerPrefs.SetString("ControlButton", button.ToString());
                break;
            }
            await Task.Delay(10);
        }
        SaveSettings();
        Reload();
        _ = DialogManager.NoButtonWindowClose();
        homeMenuSystem.DisableChangeVisible = false;
    }

    public void SaveSettings()
    {
        StatusText.text = "設定を書き込んでいます。";
        PlayerPrefs.Save();
        StatusText.text = "設定の書き込みに成功しました。";
        Invoke("SaveSettingsA", 2.0F);
    }

    void SaveSettingsA()
    {
        StatusText.text = "";
    }

    public void Exit()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 1)
            {
                DialogManager.ShowDialogNoButton(null, "しばらくお待ちください。", "終了処理中");
                await Task.Delay(3000);
                util.ApplicationQuit();
            }
        });
        DialogManager.ShowDialogSelect(unityEvent, "終了してもよろしいですか？");
    }
}
