using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRChatApi.Classes;

public class VRChat_FriendInfo : MonoBehaviour
{
    [SerializeField]
    private DialogManager DialogManager;
    [SerializeField]
    private GameObject FriendInfo;
    [SerializeField]
    private Text TitleText;
    [SerializeField]
    private Text StatusText;
    [SerializeField]
    private Text StatusDescriptionText;
    [SerializeField]
    private RawImage Image;
    [SerializeField]
    private GameObject FriendsPanel;
    [SerializeField]
    private GameObject TopMenuButtons;
    [SerializeField]
    private GameObject FriendInfoText;
    [SerializeField]
    private VRChat_Main Main;
    [SerializeField]
    private InputField UserId;
    [SerializeField]
    private Text PlatformText;
    [SerializeField]
    private InputField FriendKey;
    [SerializeField]
    private Text RankText;
    [SerializeField]
    private Text LocationText;
    [SerializeField]
    private VRChat_InstanceInfo VRChat_InstanceInfo;

    UnityEvent Callback;
    UserBriefResponse user;
    WorldResponse location;
    WorldInstanceResponse locationInstance;

    public async void ShowFriendInfo(string userId, UnityEvent callback = null)
    {
        Callback = callback;
        TitleText.text = "取得中 (" + userId + ")";
        LocationText.text = "現在のワールドを取得しています";
        Image.texture = null;

        FriendInfo.SetActive(true);
        FriendsPanel.SetActive(false);
        TopMenuButtons.SetActive(false);
        FriendInfoText.SetActive(false);

        user = await Main.api.UserApi.GetById(userId);
        TitleText.text = user.displayName + " (" + user.username + ")";
        switch (user.status)
        {
            case "busy":
                StatusText.text = "ステータス: Do Not Disturb";
                StatusText.color = new Color(1.0F, 0.0F, 0.0F);
                break;

            case "ask me":
                StatusText.text = "ステータス: Ask Me";
                StatusText.color = new Color(0.8F, 0.6F, 0.0F);
                break;

            case "active":
                StatusText.text = "ステータス: Online";
                StatusText.color = new Color(0.0F, 0.6F, 0.0F);
                break;

            case "join me":
                StatusText.text = "ステータス: Join Me";
                StatusText.color = new Color(0.0F, 0.75F, 1.0F);
                break;
        }
        StatusDescriptionText.text = user.statusDescription;
        UserId.text = user.id;
        switch (user.last_platform)
        {
            case "standalonewindows":
                PlatformText.text = "使用端末: PC";
                break;

            case "android":
                PlatformText.text = "使用端末: Quest";
                break;
        }
        FriendKey.text = user.friendKey;

        RankText.text = "ランク: Visitor";
        RankText.color = new Color(0.75F, 0.75F, 0.75F);
        if (user.tags.Contains("system_trust_basic"))
        {
            RankText.text = "ランク: New User";
            RankText.color = new Color(0.0F, 0.5F, 1.0F);
        }
        if (user.tags.Contains("system_trust_known"))
        {
            RankText.text = "ランク: User";
            RankText.color = new Color(0.0F, 0.75F, 0.0F);
        }
        if (user.tags.Contains("system_trust_trusted"))
        {
            RankText.text = "ランク: Known User";
            RankText.color = new Color(1.0F, 0.5F, 0.0F);
        }
        if (user.tags.Contains("system_trust_veteran"))
        {
            RankText.text = "ランク: Trusted User";
            RankText.color = new Color(0.75F, 0.0F, 0.75F);
        }
        if (user.tags.Contains("system_trust_legend"))
        {
            RankText.text = "ランク: Trusted User (Veteran)";
            RankText.color = new Color(0.75F, 0.0F, 0.75F);
        }

        try
        {
            if (user.location == "private")
            {
                LocationText.text = "プライベートワールド";
            }
            else
            {
                location = await Main.api.WorldApi.Get(user.location.Split(':').First());
                locationInstance = await Main.api.WorldApi.GetInstance(user.location.Split(':').First(), user.location);
                LocationText.text = location.name + "#" + locationInstance.name.Split(':')[1];

                if (locationInstance.id.Contains("friends"))
                    LocationText.text += "\n(Friends)";
                else if (locationInstance.id.Contains("hidden"))
                    LocationText.text += "\n(Friends+)";
                else
                    LocationText.text += "\n(Public)";
            }
        }
        catch
        { LocationText.text = "取得失敗"; }
        FriendInfoText.SetActive(true);
        StartCoroutine(GetTexture(user.currentAvatarImageUrl));
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

    public void HideFriendInfo()
    {
        FriendInfo.SetActive(false);
        FriendsPanel.SetActive(true);
        TopMenuButtons.SetActive(true);

        if (Callback != null)
            Callback.Invoke();
    }

    public void ShowInstance()
    {
        if (user.location == "private" || LocationText.text == "取得失敗")
            return;
        HideFriendInfo();
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            ShowFriendInfo(user.id);
        });
        _ = VRChat_InstanceInfo.ShowInstanceInfo(location.id, locationInstance.name, unityEvent);
    }
}
