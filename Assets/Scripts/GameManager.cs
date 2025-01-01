using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject Note; // タップやフリックの対象となるボタン
    [SerializeField] private GameObject Circle; // タップ用のノートオブジェクト
    [SerializeField] private GameObject LongStart; // ロングノート用のノートオブジェクト
    [SerializeField] private GameObject LongFinish;
    [SerializeField] private GameObject Flick; // フリック用のノートオブジェクト
    [SerializeField] public Text comboText; // コンボ数を表示するテキスト
    [SerializeField] private Text scoreText; // スコアを表示するテキスト
    public string CSVFileName; // 読み込むCSVファイルの名前

    private List<GameObject> Buttons = new List<GameObject>();
    private List<float>[] TimingLists = new List<float>[2]; // 各ボタンに対応するタイミングリスト
    private List<float> TimingList_all = new List<float>(); // 各ボタンに対応するタイミングリスト
    private List<int> WhichButton = new List<int>(); // どのボタンに対応するかを格納するリスト
    public List<string> NoteType = new List<string>(); // ノートの種類（タップ、フリック、ロング）を格納するリスト
    public List<string>[] NoteType2 = new List<string>[2]; // それぞれのノートに対応するノーツの種類を格納するリスト
    private List<GameObject>[] Notes = new List<GameObject>[2]; // 各ボタンに対応するノートのリスト
    public static Dictionary<string, int> PlayResult;

    int CSVIndex = 0; // 現在処理中のCSVのインデックス
    public int[] Indexes = new int[2]; // 各ボタンのノートのインデックス
    public int Combo = 0; // 現在のコンボ数

    public int Score = 0; //現在のスコア
    private int maxScore = 1000000;
    private int perfectScore; // perfectが出た時のスコア
    private float greatScoreRatio = 0.8f; //Greatの時のperfectに対するスコアの割合
    private float goodScoreRatio = 0.5f;// Goodの時のperfectに対するスコアの割合
    private int scoreRemainder = 0; //スコア合計がmaxScoreになるための端数

    public static float gametime; // ゲーム内の現在の時間
    float flickdistancemin = 0.01f; // フリックの最低移動距離
    float flicktimeMax = 0.3f; // フリックの最大許容時間
    private float flicktimeLimit = 1.5f;

    private float maxDuration = 0.6f; // コルーチンから抜け出すためのタイムアウト


string gamestat = "start"; // ゲームの状態（スタート、プレイ中など）

    bool[] isProcessing = new bool[2] { true, true }; // 各ボタンがタッチされているかどうか

    NotesManager[] notesManager = new NotesManager[2]; // 各ボタンに対応するノートマネージャー


    public static GameManager Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // インスタンスを設定
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は新しいオブジェクトを破棄
        }
    }

    EffectandSoundManager EandSManager;

    private void Start()
    {
        GameObject object1 = Instantiate(Note, new Vector3(2, -0.8f, 0), Quaternion.identity);
        GameObject object2 = Instantiate(Note, new Vector3(-2, -0.8f, 0), Quaternion.identity);
        Buttons.Add(object1);
        Buttons.Add(object2);


        // 各ボタンに対応するノートリストを初期化
        for (int i = 0; i < Buttons.Count; i++)
        {
            Notes[i] = new List<GameObject>();
            TimingLists[i] = new List<float>(); // 各ボタンに対応するタイミングリストを初期化
            Indexes[i] = 0; // インデックスを0で初期化
            NoteType2[i] = new List<string>();
        }
        TimingList_all = new List<float>();

        // 各ボタンに対応するNotesManagerを追加
        notesManager[0] = Buttons[0].AddComponent<NotesManager>();
        notesManager[1] = Buttons[1].AddComponent<NotesManager>();

        // CSVファイルを読み込み、音楽の再生を開始する
        ReadCSV(CSVFileName);
        

        PlayResult = new Dictionary<string, int>(){
        {"Perfect", 0},
        { "Great", 0},
        { "Good", 0},
        { "Miss", 0},
        };
        EandSManager = EffectandSoundManager.Instance;

        perfectScore = maxScore / TimingList_all.Count;
        scoreRemainder = maxScore - perfectScore * TimingList_all.Count;



    }

    private void Update()
    {
        // ゲームの状態による処理の分岐
        switch (gamestat)
        {
            case "start":
                // 音楽が再生されたらゲームを開始する
                EandSManager.PlaySound("shining_star");
                if (EandSManager.audioSources["shining_star"].isPlaying)
                {
                    gamestat = "playing";
                }
                break;

            case "playing":
                // ゲームの時間を更新
                gametime = (float)EandSManager.audioSources["shining_star"].timeSamples / EandSManager.audioSources["shining_star"].clip.frequency;

                // ノートのスポーンタイミングかどうかをチェック
                for (int i = 0; i < 2; i++)
                {
                    if (CSVIndex < TimingList_all.Count && gametime > TimingList_all[CSVIndex] - 1)
                    {
                        // ノートを生成する
                        GenerateNote();
                    }
                }

                // タッチが行われたときの処理
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        HandleTouch(touch);

                    }
                }

                if (Indexes[0] + Indexes[1] == TimingList_all.Count)
                {
                    StartCoroutine(LoadResultScene());
                }

                // ノートの有効範囲をチェックし、範囲外ならコンボをリセット
                break;
        }
    }

    private void GenerateNote()　//csvに書いてあるノーツの種類に対応したノーツを生成
    {
        GameObject noteObject = null;

        if (NoteType[CSVIndex] == "tap")
        {
            noteObject = Instantiate(Circle, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.identity);
        }
        else if (NoteType[CSVIndex] == "flick1")
        {
            noteObject = Instantiate(Flick, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.identity);
        }
        else if (NoteType[CSVIndex] == "flick2")
        {
            noteObject = Instantiate(Flick, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.Euler(0, 0, 90));
        }
        else if (NoteType[CSVIndex] == "flick3")
        {
            noteObject = Instantiate(Flick, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.Euler(0, 0, 180));
        }
        else if (NoteType[CSVIndex] == "flick4")
        {
            noteObject = Instantiate(Flick, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.Euler(0, 0, 270));
        }
        else if (NoteType[CSVIndex] == "longstart")
        {
            noteObject = Instantiate(LongStart, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.identity);
        }
        else if (NoteType[CSVIndex] == "longfinish")
        {
            noteObject = Instantiate(LongFinish, Buttons[WhichButton[CSVIndex]].transform.position, Quaternion.identity);

        }

        // 生成したノートオブジェクトをリストに追加し、ボタンに紐づける
        if (noteObject != null)
        {
            Notes[WhichButton[CSVIndex]].Add(noteObject);
            CircleManager circle = noteObject.GetComponent<CircleManager>();
            circle.SetTrackButton(Buttons[WhichButton[CSVIndex]], WhichButton[CSVIndex]);
        }

        // CSVのインデックスを増やす""
        CSVIndex++;

    }

    private void HandleTouch(Touch touch)
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

        for (int j = 0; j < Buttons.Count; j++)
        {
            Vector2 Buttontransform = new Vector2(Buttons[j].transform.position.x, Buttons[j].transform.position.y);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == Buttons[j] && isProcessing[j])
                {
                    if (Indexes[j] <= Notes[j].Count)
                    {
                        isProcessing[j] = false;
                        StartCoroutine(NoteTap(touch, j));　//判定の処理を開始
                    }
                    else
                    {
                        Debug.LogWarning($"Before Destroy: Index {Indexes[j]}, Notes Count {Notes[j].Count}");
                    }
                }
                
            }
        }

    }



    IEnumerator NoteTap(Touch touch, int buttonIndex)
    {
        // ノートのインデックスが範囲外の場合、処理を終了
        if (Indexes[buttonIndex] >= Notes[buttonIndex].Count)
        {
            //Debug.LogWarning("NoteTap: Index out of range for button " + buttonIndex);
            yield break;
        }

        string notetype = NoteType2[buttonIndex][Indexes[buttonIndex]];
        float starttime = gametime;
        Vector2 startpos = touch.position;

        int fingerId = touch.fingerId;



        switch (notetype)
        {
            case "tap":
                // タップ判定
                CheckTiming(Indexes[buttonIndex], gametime, buttonIndex);
                EandSManager.PlaySound("tap_sound");
                EandSManager.PlayEffect("tapEffect", Buttons[buttonIndex].transform.position);             
                while (true)
                {
                    bool touchEnded = true;

                    if (gametime - TimingLists[buttonIndex][Indexes[buttonIndex]] > maxDuration)
                    {
                        break;
                    }

                    // 対応する指のタッチを探す
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch newtouch = Input.GetTouch(i);
                        if (newtouch.fingerId == fingerId)
                        {
                            touchEnded = false;
                            break;

                        }
                    }
                    if (touchEnded)
                    {
                        break;
                    }
                    yield return null; // 毎フレーム待機
                }
                isProcessing[buttonIndex] = true;
                break;

            case "flick1":
            case "flick2":
            case "flick3":
            case "flick4":

                // フリック判定
                bool flickcheck = false;
                while (true)
                {
                    bool touchEnded = true;

                    if (gametime - TimingLists[buttonIndex][Indexes[buttonIndex]] > maxDuration)
                    {
                        break;
                    }
                    // 対応する指のタッチを探す
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch newtouch = Input.GetTouch(i);
                        if (newtouch.fingerId == fingerId)
                        {
                            touchEnded = false;

                            // フリック方向の計算
                            Vector2 moveVector = newtouch.position - startpos;

                            // フリック判定：タッチが移動しており、指定時間内で移動距離が一定以上かつ、フェーズがMovedまたはEnded
                            if (gametime - starttime < flicktimeMax && moveVector.magnitude > flickdistancemin && (newtouch.phase == TouchPhase.Moved || newtouch.phase == TouchPhase.Ended))
                            {
                                FlickCheck(notetype, buttonIndex, moveVector);
                                flickcheck = true;
                                break;

                            }
                        }
                        
                    }

                    if (flickcheck)
                    {
                        break;
                    }

                    // タッチが終了した場合、ノーツを破壊
                    if (touchEnded && !flickcheck || flicktimeLimit < gametime - starttime)
                    {
                        isProcessing[buttonIndex] = true;
                        GameObject DestroyedNote = Notes[buttonIndex][Indexes[buttonIndex]];
                        Vector3 noteScale = DestroyedNote.transform.localScale;
                        noteScale.x = 0.9f;
                        DestroyedNote.transform.localScale = noteScale;
                        Destroy(DestroyedNote);
                        break;
                    }

                    // 次のフレームまで待機
                    yield return null;
                }
                break;

            case "longstart":
                // ロングノートの開始判定
                CheckTiming(Indexes[buttonIndex], gametime, buttonIndex);

                Vector2 currentVector = notesManager[buttonIndex].rb2d.velocity;
                notesManager[buttonIndex].rb2d.velocity = Vector2.zero;
                notesManager[buttonIndex].ChangeNoteColor(Color.white, 1, true);
                EandSManager.PlaySound("tap_sound");
                EandSManager.PlayEffect("LongEffect", Buttons[buttonIndex].transform.position);




                float testcount = 0;
                // タッチが終了するまで待機
                while (true)
                {
                    if (gametime - TimingLists[buttonIndex][Indexes[buttonIndex]] > maxDuration)
                    {
                        break;
                    }

                    bool touchEnded = true;
                    testcount += Time.deltaTime;

                    // 対応する指のタッチを探す
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch newtouch = Input.GetTouch(i);
                        if (newtouch.fingerId == fingerId)
                        {
                            touchEnded = false;
                            break;
                        }
                    }
                    if (touchEnded)
                    {
                        break;
                    }
                    notesManager[buttonIndex].rb2d.velocity = Vector2.zero;
                    if (testcount > 10)
                    {
                        Debug.Log("looping!");
                        testcount = 0;
                    }
                    yield return null; 


                }


                notesManager[buttonIndex].rb2d.velocity = currentVector;

                if(Notes[buttonIndex].Count <= Indexes[buttonIndex])
                {
                    Indexes[buttonIndex]++;
                    PlayResult["Miss"]++;
                    isProcessing[buttonIndex] = true;

                }
                else
                {
                    CheckTiming(Indexes[buttonIndex], gametime, buttonIndex);   
                }
                notesManager[buttonIndex].ChangeNoteColor(Color.white, 0.5f, false);
                isProcessing[buttonIndex] = true;
                EandSManager.PlaySound("tap_sound");
                EandSManager.StopEffect("LongEffect");

                break;


        }

        isProcessing[buttonIndex] = true;
    }


    private void CheckTiming(int index, float timing, int buttonIndex) //ノーツを破壊して、コンボを管理
    {
        if (index < Notes[buttonIndex].Count && Notes[buttonIndex][index] != null)
        {
            GameObject note = Notes[buttonIndex][index];
            Destroy(note); //noteが破壊される時、CircleManagerでコンボを管理
            Combo++;
            comboText.text = Combo.ToString();

            Score = (int)(PlayResult["Perfect"] * perfectScore +
                          PlayResult["Great"] * perfectScore * greatScoreRatio +
                          PlayResult["Good"] * perfectScore * goodScoreRatio);
            scoreText.text = Score.ToString("d7");
        }

    }

    

    private void FlickCheck(string flicktype, int buttonIndex, Vector3 moveVector) //flickの種類に応じてオブジェクトが動く方向を管理
    {
        bool isFlickSuccess = false;

        if (moveVector.x < 0 && moveVector.y > 0 && flicktype == "flick1")
        {
            notesManager[buttonIndex].rb2d.velocity = new Vector2(-1, 1);
            isFlickSuccess = true;
        }
        else if (moveVector.x < 0 && moveVector.y < 0 && flicktype == "flick2")
        {
            notesManager[buttonIndex].rb2d.velocity = new Vector2(-1, -1);
            isFlickSuccess = true;
        }
        else if (moveVector.x > 0 && moveVector.y < 0 && flicktype == "flick3")
        {
            notesManager[buttonIndex].rb2d.velocity = new Vector2(1, -1);
            isFlickSuccess = true;
        }
        else if (moveVector.x > 0 && moveVector.y > 0 && flicktype == "flick4")
        {
            notesManager[buttonIndex].rb2d.velocity = new Vector2(1, 1);
            isFlickSuccess = true;
        }
        else //フリックが失敗したら、missとしてカウントを増やす
        {
            isProcessing[buttonIndex] = true;
            GameObject DestroyedNote = Notes[buttonIndex][Indexes[buttonIndex]];
            Vector3 noteScale = DestroyedNote.transform.localScale;
            noteScale.x = 0.9f;
            DestroyedNote.transform.localScale = noteScale;
            Destroy(DestroyedNote);
        }

        if (isFlickSuccess) //フリックが成功していたら、判定を行う
        {
            isProcessing[buttonIndex] = true;
            CheckTiming(Indexes[buttonIndex], gametime, buttonIndex);
            EandSManager.PlaySound("flick_sound");
            EandSManager.PlayEffect("flickEffect", Buttons[buttonIndex].transform.position);
        }
    }


    void ReadCSV(string csvFileName) 
    {
        // CSVファイルのパスを取得
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        if (File.Exists(filePath))
        {
            // CSVファイルを読み込み、タイミングリスト、ボタン、ノートタイプを設定
            string[] csvLines = File.ReadAllLines(filePath);

            foreach (string line in csvLines)
            {
                string[] rowData = line.Split(',');
                int buttonIndex = rowData[0] == "button1" ? 0 : 1;

                TimingLists[buttonIndex].Add(Convert.ToSingle(rowData[1])); // ボタンに対応するタイミングリストに追加
                TimingList_all.Add(Convert.ToSingle(rowData[1]));
                WhichButton.Add(buttonIndex);
                NoteType.Add(rowData[2]);
                NoteType2[buttonIndex].Add(rowData[2]);
            }
        }
        else
        {
            Debug.LogError("CSV file not found at " + filePath);
        }
    }

    IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(8);
        SceneManager.LoadScene(2);
        yield break;
    }







}