using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRChatApi.Classes;

public class VRChat_InstanceInfo : MonoBehaviour
{
    [SerializeField]
    private DialogManager DialogManager;
    [SerializeField]
    private GameObject InstanceInfo;
    [SerializeField]
    private GameObject FriendsPanel;
    [SerializeField]
    private Text TitleText;
    [SerializeField]
    private RawImage Image;
    [SerializeField]
    private GameObject TopMenuButtons;
    [SerializeField]
    private VRChat_Main Main;
    [SerializeField]
    private GameObject InstanceInfoText;
    [SerializeField]
    private InputField WorldId;
    [SerializeField]
    private InputField InstanceId;
    [SerializeField]
    private Text TypeText;
    [SerializeField]
    private Text DescriptionText;
    [SerializeField]
    private Text[] InstanceButtonsText;
    [SerializeField]
    private Button[] InstanceButtons;
    [SerializeField]
    private Text PageCount;
    [SerializeField]
    private VRChat_FriendInfo VRChat_FriendInfo;

    WorldResponse world;
    WorldInstanceResponse worldInstance;
    UnityEvent Callback;
    bool NewInstance = false;

    public async Task ShowInstanceInfo(string worldId, string instanceId, UnityEvent callback = null, bool isNewInstance = false)
    {
        CurrentPage = 0;
        Callback = callback;
        TitleText.text = "取得中 (" + instanceId + ")";
        Image.texture = null;

        InstanceInfo.SetActive(true);
        FriendsPanel.SetActive(false);
        TopMenuButtons.SetActive(false);
        InstanceInfoText.SetActive(false);

        for (int i1 = 0; i1 < InstanceButtonsText.Length; i1++)
        {
            InstanceButtons[i1].gameObject.SetActive(false);
        }

        world = await Main.api.WorldApi.Get(worldId);
        worldInstance = await Main.api.WorldApi.GetInstance(worldId, instanceId.Split(':')[1]);

        WorldId.text = world.id;
        InstanceId.text = worldInstance.name;
        TitleText.text = world.name + "#" + worldInstance.name;
        if (worldInstance.id.Contains("hidden"))
            TypeText.text = "Friends of guests (Friends+)";
        else if (worldInstance.id.Contains("friends"))
            TypeText.text = "Friends Only (Friends)";
        else
            TypeText.text = "Public";

        string text = "";
        try
        {
            text = world.instances.Where(a => a.id.Contains(worldInstance.name)).First().occupants + "人 / " + world.capacity + "人";
            NewInstance = false;
        }
        catch
        {
            NewInstance = isNewInstance;
            if (isNewInstance)
                text = "0人 / " + world.capacity + "人";
            else
                text = "満員 / " + world.capacity + "人";
        }
        string OwnerUser = "";
        try
        {
            OwnerUser = "\n管理者: " + (await Main.api.UserApi.GetById(worldInstance.hidden)).displayName;
        }
        catch
        { OwnerUser = "\n管理者: " + world.authorName; }
        DescriptionText.text = world.name + "\n" + text + " (" + (world.capacity * 2) + "人)" + OwnerUser + "\n作成者: " + world.authorName + "\n" + world.description;
        InstanceInfoText.SetActive(true);

        _ = LoadInstances();
        StartCoroutine(GetTexture(world.imageUrl));
    }

    List<WorldInstanceResponse> worldInstanceResponses = new List<WorldInstanceResponse>();
    bool Loading = false;
    bool CancelLoad = false;
    public async Task LoadInstances()
    {
        CancelLoad = true;
        while (Loading)
        {
            await Task.Delay(10);
        }
        CancelLoad = false;
        Loading = true;
        for (int i1 = 0; i1 < InstanceButtonsText.Length; i1++)
        {
            InstanceButtons[i1].gameObject.SetActive(false);
        }

        int i = CurrentPage * InstanceButtonsText.Length;
        int count = 0;
        int lastPage = world.instances.Count / InstanceButtonsText.Length + (world.instances.Count % InstanceButtonsText.Length == 0 ? 0 : 1) - 1;
        PageCount.text = CurrentPage + 1 + " / " + (world.instances.Count / InstanceButtonsText.Length + (world.instances.Count % InstanceButtonsText.Length == 0 ? 0 : 1));
        PageCount.text += "\n(" + (InstanceButtonsText.Length * CurrentPage + 1) + " ～ " + (CurrentPage >= lastPage ? world.instances.Count : (InstanceButtonsText.Length * CurrentPage + InstanceButtonsText.Length)) + " / " + world.instances.Count + ")";
        worldInstanceResponses.Clear();
        for (; i < world.instances.Count && count < InstanceButtonsText.Length; i++)
        {
            if (CancelLoad)
                break;
            WorldInstanceResponse worldInstance = await Main.api.WorldApi.GetInstance(world.id, world.instances[i].id);
            string instanceType = "";
            if (worldInstance.id.Contains("hidden"))
                instanceType = "Friends+";
            else if (worldInstance.id.Contains("friends"))
                instanceType = "Friends";
            else
                instanceType = "Public";
            string memberCount = "人数の取得に失敗しました";
            try
            {
                memberCount = world.instances[i].occupants + "人";
            }
            catch
            { }
            string OwnerUser = "";
            try
            {
                OwnerUser = "\n" + (await Main.api.UserApi.GetById(worldInstance.hidden)).displayName;
            }
            catch
            { OwnerUser = "\n" + world.authorName; }
            InstanceButtonsText[count].text = "#" + worldInstance.name + " (" + instanceType + ")" + OwnerUser + "\n" + memberCount;
            InstanceButtons[count].gameObject.SetActive(true);
            worldInstanceResponses.Add(worldInstance);
            count++;
        }

        for (; count < InstanceButtonsText.Length; count++)
        {
            InstanceButtons[count].gameObject.SetActive(false);
        }
        Loading = false;
    }

    int CurrentPage = 0;

    public async void NextPage()
    {
        if (CurrentPage + 1 == world.instances.Count / InstanceButtonsText.Length + (world.instances.Count % InstanceButtonsText.Length == 0 ? 0 : 1))
            return;
        CurrentPage++;
        await LoadInstances();
    }

    public async void PrevPage()
    {
        if (CurrentPage == 0)
            return;
        CurrentPage--;
        await LoadInstances();
    }

    public async void FirstPage()
    {
        CurrentPage = 0;
        await LoadInstances();
    }

    public async void LastPage()
    {
        CurrentPage = world.instances.Count / InstanceButtonsText.Length + (world.instances.Count % InstanceButtonsText.Length == 0 ? 0 : 1) - 1;
        await LoadInstances();
    }

    public async void Reload()
    {
        await ShowInstanceInfo(world.id, worldInstance.id, null, NewInstance);
    }

    public void Create()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 2)
                return;
            string instanceId = DialogManager.ResultText;
            Guid g = Guid.NewGuid();
            string pass = g.ToString("N").Substring(0, 16);
            if (DialogManager.Result == 4)
                instanceId += "~hidden(" + Main.VRChatCurrentUser.id + ")~nonce(" + pass + ")";
            else if (DialogManager.Result == 5)
                instanceId += "~friends(" + Main.VRChatCurrentUser.id + ")~nonce(" + pass + ")";
            _ = ShowInstanceInfo(world.id, world.id + ":" + instanceId, null, true);
            //await Main.api.NotificationApi.SendInvite(Main.VRChatCurrentUser.id, world.id + ":" + instanceId);
            //DialogManager.ShowDialog(null, world.name + "#" + instanceId.Split('~').First() + "へのInviteを" + Main.VRChatCurrentUser.displayName + "に送信しました。", "成功");
        });
        DialogManager.ShowDialogInput(unityEvent, "インスタンスの名前を入力し、作成したいインスタンスの種類を選択してください。Invite+およびInviteのインスタンスはここでは作成することができません。", "インスタンスの作成", "null", "キャンセル", UnityEngine.Random.Range(0, 100000).ToString(), false, "Public", "Friends+", "Friends");
    }

    public IEnumerator GetTexture(string uri)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
            Image.texture = null;
        else
        {
            Image.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }

    public void HideInstanceInfo()
    {
        InstanceInfo.SetActive(false);
        FriendsPanel.SetActive(true);
        TopMenuButtons.SetActive(true);

        if (Callback != null)
            Callback.Invoke();
    }

    public void SendInviteToMyself()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 1)
            {
                await Main.api.NotificationApi.SendInvite(Main.VRChatCurrentUser.id, worldInstance.id);
                DialogManager.ShowDialog(null, world.name + "#" + worldInstance.name + "へのInviteを" + Main.VRChatCurrentUser.displayName + "に送信しました。", "成功");
            }
        });
        
        DialogManager.ShowDialogSelect(unityEvent, world.name + "#" + worldInstance.name + "へのInviteを" + Main.VRChatCurrentUser.displayName + "に送信しますか？");
    }

    public async void ShowInstanceInfo(int index)
    {
        try
        {
            DialogManager.ShowDialogNoButton(null, "情報を更新しています。");
            ShowInstanceInfo(world.id, worldInstanceResponses[index].id);
            await DialogManager.NoButtonWindowClose();
        }
        catch
        { await DialogManager.NoButtonWindowClose(); }
    }

    public void ShowAdmin()
    {
        HideInstanceInfo();
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowInstanceInfo(world.id, worldInstance.id);
        });
        VRChat_FriendInfo.ShowFriendInfo(worldInstance.hidden ?? world.authorId, unityEvent);
    }

    public void ShowAuthor()
    {
        HideInstanceInfo();
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowInstanceInfo(world.id, worldInstance.id);
        });
        VRChat_FriendInfo.ShowFriendInfo(world.authorId, unityEvent);
    }
}
