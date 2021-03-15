using EasyLazyLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRChatApi;
using VRChatApi.Classes;

public class VRChat_Main : MonoBehaviour
{
    [SerializeField]
    private SettingsManager SettingsManager;
    [SerializeField]
    private DialogManager DialogManager;
    [SerializeField]
    private Text[] FriendButtonsText;
    [SerializeField]
    private Text[] FriendWorldButtonsText;
    [SerializeField]
    private Text[] FriendStatus;
    [SerializeField]
    private GameObject[] Friend;
    [SerializeField]
    private Text PageCount;
    [SerializeField]
    private VRChat_FriendInfo VRChat_FriendInfo;
    [SerializeField]
    private VRChat_InstanceInfo VRChat_InstanceInfo;

    public UserResponse VRChatCurrentUser;
    private int CurrentFriendPage = 0;

    public VRChatApi.VRChatApi api = null;
    EasyOpenVRUtil util = new EasyOpenVRUtil();

    void Start()
    {
        util.Init();
        _ = Login();
    }

    void Update()
    {

    }

    async Task Login()
    {
        string id = PlayerPrefs.GetString("AccountID", "");
        string password = PlayerPrefs.GetString("AccountPassword", "");
        if (id == "" || password == "")
        {
            FailedLogin();
            return;
        }
        DialogManager.ShowDialogNoButton(null, "VRChatアカウント: " + id + "としてログインしています。", "ログイン中");
        api = new VRChatApi.VRChatApi(id, password);
        api.UserApi.Username = id;
        api.UserApi.Password = password;
        VRChatCurrentUser = await api.UserApi.Login();
        await Task.Delay(2000);
        await DialogManager.NoButtonWindowClose();
        if (VRChatCurrentUser == null)
        {
            FailedLogin();
            return;
        }
        CurrentFriendPage = 0;
        Cache.Clear();
        await LoadFriends();
    }

    bool CancelLoad = false;
    bool Loading = false;
    Dictionary<string, (UserBriefResponse, WorldResponse, WorldInstanceResponse)> Cache = new Dictionary<string, (UserBriefResponse, WorldResponse, WorldInstanceResponse)>();
    List<string> PageUserKey = new List<string>();
    List<string> PageWorldKey = new List<string>();
    List<string> friends = new List<string>();
    Dictionary<string, UserBriefResponse> users = new Dictionary<string, UserBriefResponse>();
    int filterType = 0;
    async Task LoadFriends()
    {
        CancelLoad = true;
        for (int i1 = 0; i1 < Friend.Length; i1++)
            Friend[i1].SetActive(false);
        while (Loading)
            await Task.Delay(100);
        CancelLoad = false;
        //DialogManager.ShowDialogNoButton(null, "フレンド一覧を取得しています。");
        int count = 0;
        if (filterType == 0)
            friends = VRChatCurrentUser.onlineFriends;
        else if (filterType == 1)
        {
            friends.Clear();
            List<FavouriteResponse> favourites = await api.FavoriteApi.GetFavourites("friend");

            DialogManager.ShowDialogNoButton(null, "お気に入りの項目を取得しています。");
            for (int i1 = 0; i1 < favourites.Count; i1++)
            {
                DialogManager.DescriptionText.text = "お気に入りの項目を取得しています。 (" + (i1 + 1) + " / " + favourites.Count + ")";
                FavouriteResponse favorite = favourites[i1];
                if (!users.ContainsKey(favorite.favoriteId))
                    users[favorite.favoriteId] = await api.UserApi.GetById(favorite.favoriteId);
                if (users[favorite.favoriteId].location != "offline")
                    friends.Add(favorite.favoriteId);
            }
            await DialogManager.NoButtonWindowClose();
        }
        if (CurrentFriendPage < 0)
            CurrentFriendPage = 0;
        int lastPage = friends.Count / FriendButtonsText.Length + (friends.Count % FriendButtonsText.Length == 0 ? 0 : 1) - 1;
        PageCount.text = CurrentFriendPage + 1 + " / " + (lastPage + 1);
        PageCount.text += "\n(" + (FriendButtonsText.Length * CurrentFriendPage + 1) + " ～ " + (CurrentFriendPage >= lastPage ? friends.Count : (FriendButtonsText.Length * CurrentFriendPage + Friend.Length)) + " / " + friends.Count + ")";
        int i;
        Loading = true;
        PageUserKey.Clear();
        PageWorldKey.Clear();
        if (CurrentFriendPage > lastPage)
            CurrentFriendPage = lastPage;
        if (friends.Count == 0)
        {
            Loading = false;
            return;
        }
        VRChatCurrentUser = await api.UserApi.Login();
        for (i = FriendButtonsText.Length * CurrentFriendPage; i < FriendButtonsText.Length * (CurrentFriendPage + 1) && i < friends.Count; i++)
        {
            try
            {
                if (CancelLoad)
                {
                    CancelLoad = false;
                    Loading = false;
                    for (int i1 = 0; i1 < Friend.Length; i1++)
                        Friend[i1].SetActive(false);
                    return;
                }
                //DialogManager.DescriptionText.text = "フレンド一覧を取得しています。(" + (count + 1) + " / " + ((CurrentFriendPage >= lastPage) && (friends.Count % Friend.Length) != 0 ? (friends.Count % Friend.Length) : Friend.Length) + ")";
                string status = "";
                if (Cache.ContainsKey(friends[i]))
                {
                    FriendButtonsText[count].text = Cache[friends[i]].Item1.displayName;
                    status = Cache[friends[i]].Item1.status;
                    if (Cache[friends[i]].Item2 == null)
                        FriendWorldButtonsText[count].text = "プライベートワールド";
                    else
                    {
                        FriendWorldButtonsText[count].text = Cache[friends[i]].Item2.name + "#" + Cache[friends[i]].Item3.name;
                        if (Cache[friends[i]].Item1.location.Contains("hidden"))
                            FriendWorldButtonsText[count].text += "\nFriends+";
                        else if (Cache[friends[i]].Item1.location.Contains("friends"))
                            FriendWorldButtonsText[count].text += "\nFriends";
                        else
                            FriendWorldButtonsText[count].text += "\nPublic";
                    }
                }
                else
                {
                    UserBriefResponse user = null;
                    if (users.ContainsKey(friends[i]))
                        user = users[friends[i]];
                    else
                        user = await api.UserApi.GetById(friends[i]);
                    WorldInstanceResponse instance = null;
                    WorldResponse location = null;
                    FriendButtonsText[count].text = user.displayName;

                    if (user.location == "private")
                        FriendWorldButtonsText[count].text = "プライベートワールド";
                    else if (user.location == "offline")
                        FriendWorldButtonsText[count].text = "オフライン";
                    else
                    {
                        instance = await api.WorldApi.GetInstance(user.location.Split(':').First(), user.location.Split(':')[1]);
                        location = await api.WorldApi.Get(user.location.Split(':').First());
                        FriendWorldButtonsText[count].text = location.name + "#" + instance.name;
                        if (user.location.Contains("hidden"))
                            FriendWorldButtonsText[count].text += "\nFriends+";
                        else if (user.location.Contains("friends"))
                            FriendWorldButtonsText[count].text += "\nFriends";
                        else
                            FriendWorldButtonsText[count].text += "\nPublic";
                    }
                    status = user.status;
                    Cache[friends[i]] = (user, location, instance);
                }

                switch (status)
                {
                    case "busy":
                        FriendStatus[count].text = "Do Not Disturb";
                        FriendStatus[count].color = new Color(1.0F, 0.0F, 0.0F);
                        break;

                    case "ask me":
                        FriendStatus[count].text = "Ask Me";
                        FriendStatus[count].color = new Color(0.8F, 0.6F, 0.0F);
                        break;

                    case "active":
                        FriendStatus[count].text = "Online";
                        FriendStatus[count].color = new Color(0.0F, 0.6F, 0.0F);
                        break;

                    case "join me":
                        FriendStatus[count].text = "Join Me";
                        FriendStatus[count].color = new Color(0.0F, 0.75F, 1.0F);
                        break;
                }
                PageUserKey.Add(friends[i]);
                if (Cache[friends[i]].Item2 != null)
                {
                    PageWorldKey.Add(Cache[friends[i]].Item2.id);
                }
                else
                {
                    PageWorldKey.Add(null);
                }
                Friend[count].SetActive(true);
                count++;
            }
            catch (Exception ex)
            {
                PageUserKey.Add(friends[i]);
                PageWorldKey.Add(null);
                FriendStatus[count].text = "";
                FriendWorldButtonsText[count].text = "";
                FriendButtonsText[count].text = "取得失敗";
                Friend[count].SetActive(true);
                count++;
            }
        }
        for (; count < Friend.Length; count++)
        {
            Friend[count].SetActive(false);
        }
        Loading = false;
        //await DialogManager.NoButtonWindowClose();
    }

    public void ReloadFriends()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 1)
            {
                Cache.Clear();
                users.Clear();
                CurrentFriendPage = 0;
                await LoadFriends();
            }
        });
        DialogManager.ShowDialogSelect(unityEvent, "フレンドリストを再読み込みしてもよろしいですか？");
    }

    public async void FirstPage()
    {
        CurrentFriendPage = 0;
        await LoadFriends();
    }

    public async void LastPage()
    {
        CurrentFriendPage = friends.Count / FriendButtonsText.Length + (friends.Count % FriendButtonsText.Length == 0 ? 0 : 1) - 1;
        await LoadFriends();
    }

    public async void NextPage()
    {
        if (CurrentFriendPage + 1 == friends.Count / FriendButtonsText.Length + (friends.Count % FriendButtonsText.Length == 0 ? 0 : 1))
            return;
        CurrentFriendPage++;
        await LoadFriends();
    }

    public async void PrevPage()
    {
        if (CurrentFriendPage == 0)
            return;
        CurrentFriendPage--;
        await LoadFriends();
    }

    public void Filter()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 1)
                filterType = 0;
            else if (DialogManager.Result == 2)
                filterType = 1;
            CurrentFriendPage = 0;
            await LoadFriends();
        });
        DialogManager.ShowDialogSelect(unityEvent, "オンラインのフレンドを絞り込む方法を選択してください。", "確認", "すべて表示", "お気に入りのみ表示");
    }

    public void ShowMe()
    {
        VRChat_FriendInfo.ShowFriendInfo(VRChatCurrentUser.id);
    }

    void FailedLogin()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(async () =>
        {
            if (DialogManager.Result == 1)
            {
                SetAccount();
            }
            else if (DialogManager.Result == 2)
            {
                DialogManager.ShowDialogNoButton(null, "しばらくお待ちください。", "終了処理中");
                await Task.Delay(3000);
                util.ApplicationQuit();
            }
        });
        DialogManager.ShowDialogSelect(unityEvent, "アカウントが設定されていないか、ログインに失敗しました。アカウントを設定してください。", "失敗", "アカウント設定", "終了");
    }

    public void SetAccount()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            PlayerPrefs.SetString("AccountID", DialogManager.ResultText);
            PlayerPrefs.Save();
            unityEvent = new UnityEvent();
            unityEvent.AddListener(async () =>
            {
                PlayerPrefs.SetString("AccountPassword", DialogManager.ResultText);
                PlayerPrefs.Save();
                await Login();
            });
            DialogManager.ShowDialogInput(unityEvent, "キーボードでVRChatアカウントのパスワードを入力してください。", "VRChatアカウントの設定", "OK", "null", PlayerPrefs.GetString("AccountPassword", ""), true);
        });
        DialogManager.ShowDialogInput(unityEvent, "キーボードでVRChatアカウントのユーザー名を入力してください。", "VRChatアカウントの設定", "OK", "null", PlayerPrefs.GetString("AccountID", ""));
    }

    public void ShowFriendInfo(int index)
    {
        VRChat_FriendInfo.ShowFriendInfo(PageUserKey[index]);
    }

    public async void ShowInstanceInfo(int index)
    {
        try
        {
            DialogManager.ShowDialogNoButton(null, "情報を更新しています。");
            UserBriefResponse user = await api.UserApi.GetById(PageUserKey[index]);
            //Cache[user.id] = (user, null, null);
            Cache.Remove(user.id);
            await LoadFriends();
            if (user.location != "private")
            {
                WorldResponse world = await api.WorldApi.Get(user.location.Split(':').First());
                WorldInstanceResponse instance = await api.WorldApi.GetInstance(user.location.Split(':').First(), user.location.Split(':')[1]);
                _ = VRChat_InstanceInfo.ShowInstanceInfo(world.id, instance.id);
            }
            await DialogManager.NoButtonWindowClose();
        }
        catch
        { await DialogManager.NoButtonWindowClose(); }
    }
}
