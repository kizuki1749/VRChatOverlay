using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRChatApi.Classes;

public class WorldManager : MonoBehaviour
{
    [SerializeField]
    private SettingsManager SettingsManager;
    [SerializeField]
    private DialogManager DialogManager;
    [SerializeField]
    private Text[] WorldButtonsText;
    [SerializeField]
    private Button[] WorldButtons;
    [SerializeField]
    private GameObject[] World;
    [SerializeField]
    private Text PageCount;
    [SerializeField]
    private VRChat_InstanceInfo VRChat_InstanceInfo;
    [SerializeField]
    private VRChat_Main VRChat_Main;
    [SerializeField]
    private TopMenuSystem TopMenuSystem;
    [SerializeField]
    private Button FilterButton;
    [SerializeField]
    private Button OrderButton;

    int CurrentPage = 0;
    bool CancelLoad = false;
    bool Loading = false;
    string SearchWord = "";
    Dictionary<string, WorldResponse> Cache = new Dictionary<string, WorldResponse>();
    WorldGroups groups = WorldGroups.Any;
    List<WorldBriefResponse> worlds;
    UserOptions UserOptions = UserOptions.Friends;
    SortOptions SortOptions = SortOptions.Popularity;
    bool Feautured = false;

    public async void Start()
    {
        while (VRChat_Main.VRChatCurrentUser == null)
            await Task.Delay(100);
        _ = LoadWorlds();
    }

    public void Reload()
    {
        _ = LoadWorlds();
    }

    public async Task LoadWorlds()
    {
        CancelLoad = true;
        for (int i1 = 0; i1 < World.Length; i1++)
            World[i1].SetActive(false);
        while (Loading)
            await Task.Delay(100);
        CancelLoad = false;
        Loading = true;
        PageCount.text = (CurrentPage + 1) + "ページ\n(" + (CurrentPage * World.Length + 1) + " ～ " + (CurrentPage * World.Length + World.Length + 1) + ")";
        if ((CurrentPage * World.Length) < 0)
        {
            Loading = false;
            return;
        }
        if (groups == WorldGroups.Any)
        {
            FilterButton.gameObject.SetActive(true);
            OrderButton.gameObject.SetActive(true);
        }
        else
        {
            FilterButton.gameObject.SetActive(false);
            OrderButton.gameObject.SetActive(false);
        }
        worlds = await VRChat_Main.api.WorldApi.Search(groups, keyword: SearchWord, offset: CurrentPage * World.Length, count: World.Length, user: UserOptions, sort: SortOptions, featured: Feautured);
        if (worlds.Count == 0)
        {
            CurrentPage--;
            Loading = false;
            _ = LoadWorlds();
            return;
        }
        for (int i = 0; i < World.Length && i < worlds.Count; i++)
        {
            if (CancelLoad)
                break;
            WorldButtonsText[i].text = worlds[i].name + "\n合計" + worlds[i].occupants + "人 / 最大" + worlds[i].capacity + "人 / 人気度: " + worlds[i].heat + " / お気に入り数: " + worlds[i].favorites + (worlds[i].visits != 0 ? (" / 閲覧数: " + worlds[i].visits) : "");
            World[i].SetActive(true);
        }
        Loading = false;
    }

    public void First()
    {
        CurrentPage = 0;
        _ = LoadWorlds();
    }

    public void Prev()
    {
        if (CurrentPage == 0)
            return;
        CurrentPage--;
        _ = LoadWorlds();
    }

    public void Next()
    {
        CurrentPage++;
        _ = LoadWorlds();
    }

    public void SetType()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            if (DialogManager.Result == 2)
                return;
            SearchWord = DialogManager.ResultText;
            switch (DialogManager.Result)
            {
                case 3: groups = WorldGroups.Any; break;
                case 4: groups = WorldGroups.Recent; break;
                case 5: groups = WorldGroups.Favorite; break;
            }
            CurrentPage = 0;
            _ = LoadWorlds();
        });
        DialogManager.ShowDialogInput(unityEvent, "検索方法を選択してください。\n検索する場合は検索ワードを入力して「すべて」を選択してください。", "検索方法", "null", "キャンセル", SearchWord, false, "すべて", "履歴", "お気に入り");
    }

    public void SetUser()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            if (DialogManager.Result == 2)
                return;
            switch (DialogManager.Result)
            {
                case 3: UserOptions = UserOptions.Friends; break;
                case 4: UserOptions = UserOptions.Me; break;
            }
            unityEvent = new UnityEvent();
            unityEvent.AddListener(() =>
            {
                switch (DialogManager.Result)
                {
                    case 1: Feautured = true; break;
                    case 2: Feautured = false; break;
                }
                CurrentPage = 0;
                _ = LoadWorlds();
            });
            DialogManager.ShowDialogSelect(unityEvent, "絞り込む方法を指定してください。", "絞り込み", "注目ワールドのみ", "すべて表示");
        });
        DialogManager.ShowDialogSelect(unityEvent, "絞り込む方法を指定してください。", "絞り込み", "null", "キャンセル", "すべて表示", "自分のワールドのみ");
    }

    public void SetOrder()
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            if (DialogManager.Result == 2)
                return;
            switch (DialogManager.Result)
            {
                case 3: SortOptions = SortOptions.Popularity; break;
                case 4: SortOptions = SortOptions.Order; break;
                case 5: SortOptions = SortOptions.Created; break;
                case 6: SortOptions = SortOptions.Updated; break;
            }

            CurrentPage = 0;
            _ = LoadWorlds();
        });
        DialogManager.ShowDialogSelect(unityEvent, "並べ替える方法を指定してください。", "絞り込み", "null", "キャンセル", "人数順", "取得順", "新しい順", "更新が新しい順");
    }

    public void ShowInstance(int index)
    {
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {
            TopMenuSystem.SetVisiblePanel(3);
        });
        _ = VRChat_InstanceInfo.ShowInstanceInfo(worlds[index].id, worlds[index].id + ":" + Random.Range(0, 100000).ToString(), unityEvent, true);
    }
}
