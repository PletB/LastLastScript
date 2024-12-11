using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FieldFiveTrigger : MonoBehaviour
{
    // TextMeshPro 텍스트 및 UI 관련 오브젝트들
    public TMP_Text dialogueText; // 대사 텍스트 표시용 TextMeshPro 텍스트
    public GameObject textBox; // 텍스트 박스 오브젝트
    public TMP_Text promptText; // "F키를 누르세요" 안내 텍스트
    public SPUM_Prefabs anim; // 플레이어 애니메이션 참조
    public AudioSource textTrueEffect; // 트리거 진입 시 재생될 사운드
    public AudioSource textFalseEffect; // 트리거 벗어날 때 재생될 사운드
    public AudioSource textEffect; // 텍스트 출력 시 재생될 사운드
    public GameObject MidleWall; // 퀘스트 수락 시 제거될 벽 오브젝트
    public GameObject SixTrigger; // 퀘스트 완료 후 활성화될 트리거 오브젝트
    public float textDisplayDelay = 0.05f; // 텍스트 한 글자씩 출력될 때의 딜레이

    // 내부 상태 관리 변수들
    private bool isPlayerInTrigger = false; // 플레이어가 트리거 내에 있는지 여부
    private bool isTextDisplaying = false; // 텍스트가 출력 중인지 여부
    private int dialogueIndex = 0; // 현재 대사의 인덱스
    private bool questAccepted = false; // 퀘스트 수락 여부
    private bool questDeclined = false; // 퀘스트 거절 여부
    private bool questCompleted = false; // 퀘스트 완료 여부
    public int monstersCaptured = 0; // 현재까지 잡은 몬스터 수
    public int requiredMonsters = 5; // 퀘스트 완료에 필요한 몬스터 수
    private QuestFieldPlayer playerScript; // 플레이어 스크립트 참조

    // 대사 배열
    private readonly string[] dialogues = new string[]
    {
        "나 다시 보니 반갑지?",
        "이제 너한테 뭘 해야할지 알려줄게.",
        "저기 보이는 선량한 시민 5명만 잡아와",
        "저 착하고 똘망똘망한 눈을 봐.. 너무 불쌍해..ㅠㅠ",
        "넌 이제부터 저걸 잡아오는거야.",
        "근데.. 솔직히 저걸 잡는다?.. 나라면 미안해서 못 잡음;;",
        "뭐 너가 잡든 못잡든 그건 내 알빠 아니지만..ㅋ",
        "암튼, 저 선량한 시민 5명을 잡아오기만 하면 돼.",
        "퀘스트 : 선량한 시민 5명 잡아오기",
        "수락하시겠습니까?",
        "뭐야;; 게임 안할거면 왜 왔어..?", // "아니오" 버튼 클릭 시
        "그냥 게임 하지마!",
        "좋아! 가서 '5명' 잡아오면 돼 어서 가!", // "예" 버튼 클릭 시
        "아직 남았어;;",
        "아직 남았다니까?!",
        "말귀를 못 알아듣네 아직 남았다고..",
        "하... 안되겠다.. 고통이 뭔지 알려줄게.. 30마리 잡아와;;",
        "오! 용케도 다 잡았네. ㅋ",
        "이제 또 해야할게 있는데.. 그건 내가 알려주기 싫고",
        "ALT 랑 F4 동시에 눌러봐.",
        "그럼 다음 퀘스트 나와. 진짜로!ㅎ",
        "정말이라니까! 내가 안꺼지게 해뒀어~",
        "나 믿고 한번만 눌러봐!",
        "진짜 껐으면 아쉬운거고 ㅋㅋㅋ^^",
        "다음 퀘스트를 받고 싶으면 다음 포탈을 타고 가.",
        "걔는 좀 친절해.ㅎㅎ",
        "아직 안갔어..? 좀 가라..;;"
    };

    public MoveCamera moveCamera; // MoveCamera 스크립트 참조
    public Button acceptButton; // "네" 버튼
    public Button declineButton; // "아니오" 버튼

    private void Start()
    {
        // 텍스트 박스와 버튼 초기화
        if (textBox != null) textBox.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (SixTrigger != null) SixTrigger.SetActive(false); // SixTrigger 시작 시 비활성화

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);

        // 버튼 클릭 이벤트 연결
        acceptButton.onClick.AddListener(OnAccept);
        declineButton.onClick.AddListener(OnDecline);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 트리거에 들어왔을 때 처리
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            // 첫 대사 출력 전 "F키를 누르세요" 텍스트 표시
            if (promptText != null && !isTextDisplaying && dialogueIndex == 0)
            {
                promptText.gameObject.SetActive(true);
                if (textTrueEffect != null)
                {
                    textTrueEffect.Play();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어가 트리거를 벗어났을 때 처리
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            // "F키를 누르세요" 텍스트 숨기기
            if (promptText != null && !isTextDisplaying && dialogueIndex == 0)
            {
                promptText.gameObject.SetActive(false);
                if (textFalseEffect != null)
                {
                    textFalseEffect.Play();
                }
            }
        }
    }

    private void Update()
    {
        // 플레이어가 F키를 눌렀을 때 대화 진행
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F) && !isTextDisplaying
            && !acceptButton.gameObject.activeSelf && !declineButton.gameObject.activeSelf)
        {
            if (questAccepted && !questCompleted && dialogueIndex >= 17)
            {
                return; // 퀘스트 진행 중에는 대화 스킵
            }

            if (dialogueIndex < dialogues.Length)
            {
                if (questAccepted && dialogueIndex < 12)
                {
                    dialogueIndex = 12; // 수락 후 대화 진행
                }

                StartCoroutine(DisplayTextWithAnimation(dialogues[dialogueIndex]));

                if (questDeclined && dialogueIndex >= 11)
                {
                    Application.Quit(); // 퀘스트 거절 시 게임 종료
                    Debug.Log("게임이 종료됐음");
                }

                dialogueIndex++;
            }
        }
    }

    public void MonsterCaptured()
    {
        // 몬스터 처치 카운트 업데이트
        if (questAccepted && !questCompleted)
        {
            monstersCaptured++;

            // 특정 대화 단계에서 요구 몬스터 수 변경
            if (dialogueIndex == 17)
            {
                requiredMonsters = 30;
            }

            // 퀘스트 완료 조건 확인
            if (monstersCaptured >= requiredMonsters)
            {
                questCompleted = true;
                StartCoroutine(CompleteQuest());
                dialogueIndex = 17; // 퀘스트 완료 후 대화 진행
            }
        }
    }

    private IEnumerator DisplayTextWithAnimation(string message)
    {
        // 텍스트 애니메이션 출력
        isTextDisplaying = true;
        if (textBox != null) textBox.SetActive(true);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.text = "";

            foreach (char letter in message)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textDisplayDelay);
                if (textEffect != null)
                {
                    textEffect.Play();
                }
            }
        }

        // 특정 대화 단계에서 카메라 줌 인
        if (dialogueIndex == 3)
        {
            moveCamera.ZoomInOnTarget();
        }

        // 대화 완료 시 버튼 표시
        if (dialogueIndex == 10)
        {
            if (acceptButton != null) acceptButton.gameObject.SetActive(true);
            if (declineButton != null) declineButton.gameObject.SetActive(true);
        }

        isTextDisplaying = false;
    }

    private void OnAccept()
    {
        // 퀘스트 수락 처리
        questAccepted = true;
        dialogueIndex = 12; // 대사 진행
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        MidleWall.SetActive(false); // 벽 제거
    }

    private void OnDecline()
    {
        // 퀘스트 거절 처리
        questDeclined = true;
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
    }

    public IEnumerator CompleteQuest()
    {
        // 퀘스트 완료 처리
        yield return new WaitForSeconds(0);

        // 모든 몬스터 비활성화
        MonsterController[] monsters = FindObjectsOfType<MonsterController>();
        foreach (MonsterController monster in monsters)
        {
            monster.DisableMonster();
        }

        // SixTrigger 활성화
        if (SixTrigger != null)
        {
            SixTrigger.SetActive(true);
        }
    }
}
