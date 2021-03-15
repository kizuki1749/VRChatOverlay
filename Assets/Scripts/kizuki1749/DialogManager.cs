using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField]
    private GameObject DialogObject;
    [SerializeField]
    private Animator Animator;
    [SerializeField]
    public Text TitleText;
    [SerializeField]
    public Text DescriptionText;
    [SerializeField]
    private Text Button1Text;
    [SerializeField]
    private Text Button2Text;
    [SerializeField]
    private Text Button3Text;
    [SerializeField]
    private Text Button4Text;
    [SerializeField]
    private Text Button5Text;
    [SerializeField]
    private Text Button6Text;
    [SerializeField]
    private Text Button7Text;
    [SerializeField]
    private Button Button1;
    [SerializeField]
    private Button Button2;
    [SerializeField]
    private Button Button3;
    [SerializeField]
    private Button Button4;
    [SerializeField]
    private Button Button5;
    [SerializeField]
    private Button Button6;
    [SerializeField]
    private Button Button7;
    [SerializeField]
    private InputField Input;

    public int Result = 0;
    public string ResultText = "";

    UnityEvent Callback;

    public void ShowDialog(UnityEvent callback, string description, string title = "確認", string buttonText = "OK")
    {
        Callback = callback;
        TitleText.text = title;
        DescriptionText.text = description;
        Button1Text.text = buttonText;
        Input.gameObject.SetActive(false);
        Button1.gameObject.SetActive(true);
        Button2.gameObject.SetActive(false);
        Button3.gameObject.SetActive(false);
        Button4.gameObject.SetActive(false);
        Button5.gameObject.SetActive(false);
        Button6.gameObject.SetActive(false);
        Button7.gameObject.SetActive(false);
        Animator.SetBool("Open", true);
    }

    public void ShowDialogSelect(UnityEvent callback, string description, string title = "確認", string button1Text = "OK", string button2Text = "キャンセル", string button3Text = "null", string button4Text = "null", string button5Text = "null", string button6Text = "null", string button7Text = "null")
    {
        Callback = callback;
        TitleText.text = title;
        DescriptionText.text = description;
        Button1Text.text = button1Text;
        Button2Text.text = button2Text;
        Button3Text.text = button3Text;
        Button4Text.text = button4Text;
        Button5Text.text = button5Text;
        Button6Text.text = button6Text;
        Button7Text.text = button7Text;
        Input.gameObject.SetActive(false);
        Button1.gameObject.SetActive(button1Text != "null");
        Button2.gameObject.SetActive(button2Text != "null");
        Button3.gameObject.SetActive(button3Text != "null");
        Button4.gameObject.SetActive(button4Text != "null");
        Button5.gameObject.SetActive(button5Text != "null");
        Button6.gameObject.SetActive(button6Text != "null");
        Button7.gameObject.SetActive(button7Text != "null");
        Animator.SetBool("Open", true);
    }

    public void ShowDialogInput(UnityEvent callback, string description, string title = "確認", string button1Text = "OK", string button2Text = "キャンセル", string defaultValue = "", bool isPassword = false, string button3Text = "null", string button4Text = "null", string button5Text = "null", string button6Text = "null", string button7Text = "null")
    {
        Callback = callback;
        TitleText.text = title;
        DescriptionText.text = description;
        Button1Text.text = button1Text;
        Button2Text.text = button2Text;
        Button3Text.text = button3Text;
        Button4Text.text = button4Text;
        Button5Text.text = button5Text;
        Button6Text.text = button6Text;
        Button7Text.text = button7Text;
        Input.gameObject.SetActive(true);
        Input.text = defaultValue;
        Input.contentType = isPassword ? InputField.ContentType.Password : InputField.ContentType.Standard;
        Button1.gameObject.SetActive(button1Text != "null");
        Button2.gameObject.SetActive(button2Text != "null");
        Button3.gameObject.SetActive(button3Text != "null");
        Button4.gameObject.SetActive(button4Text != "null");
        Button5.gameObject.SetActive(button5Text != "null");
        Button6.gameObject.SetActive(button6Text != "null");
        Button7.gameObject.SetActive(button7Text != "null");
        Animator.SetBool("Open", true);
    }

    public void ShowDialogNoButton(UnityEvent callback, string description, string title = "処理中")
    {
        Callback = callback;
        TitleText.text = title;
        DescriptionText.text = description;
        Input.gameObject.SetActive(false);
        Button1.gameObject.SetActive(false);
        Button2.gameObject.SetActive(false);
        Button3.gameObject.SetActive(false);
        Button4.gameObject.SetActive(false);
        Button5.gameObject.SetActive(false);
        Button6.gameObject.SetActive(false);
        Button7.gameObject.SetActive(false);
        Animator.SetBool("Open", true);
    }

    public async Task NoButtonWindowClose()
    {
        Result = 0;
        Animator.SetBool("Open", false);
        await Task.Delay(150);
        Invoke("SetSelectedButtonA", 0.15F);
    }

    public void SetSelectedButton(int result)
    {
        Result = result;
        ResultText = Input.text;
        Animator.SetBool("Open", false);

        Invoke("SetSelectedButtonA", 0.15F);
    }

    public void SetSelectedButtonA()
    {
        if (Callback != null)
            Callback.Invoke();
    }

    public void RunTest1()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowDialog(null, Result.ToString(), "実行結果");
        });
        ShowDialog(unityEvent, "テストダイアログボックス1", "テスト");
    }

    public void RunTest2()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowDialog(null, Result.ToString(), "実行結果");
        });
        ShowDialogSelect(unityEvent, "テストダイアログボックス2", "テスト");
    }

    public async void RunTest3()
    {
        ShowDialogNoButton(null, "テストダイアログボックス3", "テスト");
        await Task.Delay(3000);
        _ = NoButtonWindowClose();
    }

    public void RunTest4()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowDialog(null, Result.ToString() + "\n" + ResultText, "実行結果");
        });
        ShowDialogInput(unityEvent, "テストダイアログボックス4", "テスト", "OK", "キャンセル", "でふぉると");
    }
}
