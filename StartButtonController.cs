using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonController : MonoBehaviour
{
    // 경고 텍스트, InputField 오브젝트
    public TMP_Text warningText;
    public TMP_Text errorText;
    public GameObject mainPlayerName;  // MainPlayerName 오브젝트 (InputField 포함)
    public GameObject realOptionButton;  // RealOptionButton 오브젝트 (InputField 포함)

    // 스크립트 참조
    private MultiTextDisplayController multiTextDisplayController;
    private OptionButtonController optionButtonController;
    private QuitButtonController quitButtonController;
    private RealOptionButtonController realOptionButtonController;

    //추가된 효과음 오디오 소스
    public AudioSource textEffectAudioSource; // Text 효과음
    public AudioSource buttonClickAudioSource; // 버튼 클릭 효과음 재생을 위한 AudioSource
    public AudioSource warningTextAudioSource; // 경고 효과음 재생을 위한 AudioSource

    // 버튼 및 애니메이션 관련 변수
    public RectTransform startButton;  // Start 버튼의 RectTransform
    public float dropDuration = 1.0f;  // 버튼이 내려오는 시간
    public float bounceFactor = 0.3f;  // 반동 크기 (얼마나 많이 튕길지)
    public float bounceSpeed = 2.0f;  // 반동 속도
    private bool isStartButtonPressed = false;  // Start 버튼이 눌렸는지 여부
    private bool isNameEntered = false;  // 플레이어가 이름을 입력했는지 여부
    private int warningCount = -1;  // Text7,8 경고 메시지 횟수
    private bool isWarningActive = false;  // Text7,8 경고 메시지가 출력 중인지 여부
    private bool isStartButtonReturn = false;  // 버튼 애니메이션이 작동중인지 여부

    void Start()
    {
        // Start 버튼 초기 위치 설정 (화면 위로)
        startButton.anchoredPosition = new Vector2(startButton.anchoredPosition.x,  Screen.height+ startButton.rect.height);

        // MultiTextDisplayController를 찾아서 참조
        multiTextDisplayController = FindObjectOfType<MultiTextDisplayController>();

        // optionButtonController 찾아서 참조
        optionButtonController = FindObjectOfType<OptionButtonController>();

        // quitButtonController 찾아서 참조
        quitButtonController = FindObjectOfType<QuitButtonController>();

        // realOptionButtonController 찾아서 참조
        realOptionButtonController = FindObjectOfType<RealOptionButtonController>();

        // 처음엔 경고 텍스트 비활성화
        warningText.gameObject.SetActive(false);

        // 처음엔 경고 텍스트 비활성화
        errorText.gameObject.SetActive(false);

        // 처음엔 InputField 비활성화
        mainPlayerName.gameObject.SetActive(false);

        // InputField의 onEndEdit 이벤트를 연결하여 이름이 입력되었는지 확인
        TMP_InputField inputField = mainPlayerName.GetComponent<TMP_InputField>();
        inputField.onEndEdit.AddListener(delegate { CheckNameInput(inputField.text); });
    }

    // Start 버튼이 눌렸을 때 호출되는 메서드
    public void OnStartButtonPressed()
    {
        // 경고 메시지가 출력 중일 때는 버튼 클릭을 무시
        if (isWarningActive || isStartButtonReturn)
        {
            return; // 함수 종료, 클릭을 무시함
        }

        // 버튼 클릭 효과음 재생
        if (buttonClickAudioSource != null)
        {
            buttonClickAudioSource.PlayOneShot(buttonClickAudioSource.clip);  // 버튼 클릭 시 효과음 재생
        }

        // Start 버튼을 처음 클릭했을 때 MoveButtonUp
        if (!isStartButtonPressed)
        {
            isStartButtonPressed = true;
            StartCoroutine(MoveButtonUp());
        }
        else
        {
            // 두 번째 클릭 시, OnInputField 코루틴 실행
            StartCoroutine(OnInputField());
        }
    }

    // Start 버튼의 대기 함수
    public IEnumerator StartButtonWait()
    {
        // Start 버튼 애니메이션 실행
        yield return StartCoroutine(DropStartButton());

        float waitTime = 0f;  // 대기 시간 누적 변수
        float maxWaitTime = 3f;  // 3초
        string firstWarningMessage = "버튼 좀 눌러줄래..?"; // 첫 번째 대기 텍스트
        string secondWarningMessage = "그 망할 버튼 좀 누르라고.. 멍청아!"; // 두 번째 경고 텍스트
        string thirdWarningMessage = "넌 버튼 누르는 법도 모르니..? 에휴.. 그냥 나가라;;"; // 세 번째 경고 텍스트
        bool displayedFirstWarning = false;  // 첫 번째 경고 텍스트가 출력되었는지 여부
        bool displayedSecondWarning = false;  // 두 번째 경고 텍스트가 출력되었는지 여부

        // Start 버튼을 누를 때까지 대기
        while (!isStartButtonPressed)
        {
            yield return null; // 매 프레임 대기

            // 클릭 대기 시간이 maxWaitTime를 초과하면 첫 번째 텍스트 출력
            waitTime += Time.deltaTime;
            if (waitTime >= maxWaitTime && !displayedFirstWarning)
            {
                displayedFirstWarning = true;  // 첫 번째 경고 텍스트가 출력됨

                yield return StartCoroutine(DisplayWarningAndReactivateButton(firstWarningMessage));

                // 대기 시간 초기화 후 두 번째 텍스트 출력 대기로 이동
                waitTime = 0f;
            }

            // 클릭 대기 시간이 maxWaitTime를 초과하면 두 번째 텍스트 출력
            if (waitTime >= maxWaitTime && displayedFirstWarning && !displayedSecondWarning)
            {
                displayedSecondWarning = true;  // 두 번째 경고 텍스트가 출력됨

                yield return StartCoroutine(DisplayWarningAndReactivateButton(secondWarningMessage));

                // 대기 시간 초기화 후 세 번째 텍스트 출력 대기로 이동
                waitTime = 0f;
            }

            // 클릭 대기 시간이 maxWaitTime를 초과하면 세 번째 텍스트 출력
            if (waitTime >= maxWaitTime && displayedSecondWarning)
            {
                // StartButton 비활성화
                startButton.gameObject.SetActive(false);

                yield return StartCoroutine(DisplayTextWithAnimation(thirdWarningMessage));

                // 텍스트 출력이 끝난 후 1초 대기
                yield return new WaitForSeconds(1f);

                // 게임 종료
                Application.Quit();
            }
        }

        // Start 버튼이 눌린 후 MultiTextDisplayController에서 텍스트를 계속 출력
        yield return StartCoroutine(multiTextDisplayController.ContinueDisplayingTextsFromIndex(4));
    }

    // Text6 후 실행 함수
    public IEnumerator StartButtonReturn()
    {
        isStartButtonReturn = true; // 버튼 애니메이션 작동 중

        // Start 버튼 애니메이션 실행
        yield return StartCoroutine(DropStartButton());

        // Option 버튼 애니메이션 실행
        yield return StartCoroutine(optionButtonController.DropOptionButton());

        // Quit 버튼 애니메이션 실행
        yield return StartCoroutine(quitButtonController.DropQuitButton());

        // RealOption 버튼 애니메이션 실행
        yield return StartCoroutine(realOptionButtonController.UpRealOptionButton());

        // InputField 활성화
        mainPlayerName.gameObject.SetActive(true);

        // Start 버튼 애니메이션 실행
        yield return StartCoroutine(OnInputField());
    }

    // Start 버튼이 아래로 내려오는 애니메이션 코루틴
    private IEnumerator DropStartButton()
    {
        float elapsedTime = 0f;
        Vector2 startPos = startButton.anchoredPosition;
        Vector2 endPos = new Vector2(startButton.anchoredPosition.x, 50f);

        // 드롭 애니메이션
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            startButton.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 정확한 위치 보정
        startButton.anchoredPosition = endPos;
        StartCoroutine(BounceStartButton());
    }

    // Start 버튼이 화면 위로 올라가는 애니메이션 코루틴
    private IEnumerator MoveButtonUp()
    {
        float elapsedTime = 0f;
        Vector2 startPos = startButton.anchoredPosition;
        Vector2 endPos = new Vector2(startButton.anchoredPosition.x, Screen.height + startButton.rect.height);

        // 올라가는 애니메이션 실행
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            startButton.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 정확한 위치 보정
        startButton.anchoredPosition = endPos;
    }

    // Start 버튼의 반동 애니메이션
    private IEnumerator BounceStartButton()
    {
        float elapsedTime = 0f;
        bool bouncing = true;

        while (bouncing)
        {
            elapsedTime += Time.deltaTime;

            // 반동 효과 계산
            float bounce = Mathf.Sin(elapsedTime * bounceSpeed) * Mathf.Exp(-elapsedTime * bounceSpeed) * bounceFactor;
            startButton.anchoredPosition = new Vector2(startButton.anchoredPosition.x, startButton.anchoredPosition.y + bounce);

            if (Mathf.Abs(bounce) < 0.01f)
            {
                bouncing = false;
            }

            yield return null;
        }
    }

    // 경고 메시지를 출력하고 StartButton을 다시 활성화하는 코루틴
    public IEnumerator DisplayWarningAndReactivateButton(string warningMessage)
    {
        // StartButton 비활성화
        startButton.gameObject.SetActive(false); 

        // 경고 텍스트 애니메이션 출력
        yield return StartCoroutine(DisplayTextWithAnimation(warningMessage));

        // 텍스트 출력이 끝난 후 1초 대기
        yield return new WaitForSeconds(1f);

        // 경고 텍스트 비활성화
        warningText.gameObject.SetActive(false);

        // StartButton 다시 활성화
        startButton.gameObject.SetActive(true);
    }

    // 한 글자씩 텍스트를 출력하는 코루틴
    private IEnumerator DisplayTextWithAnimation(string message)
    {
        warningText.gameObject.SetActive(true);  // 경고 텍스트 활성화
        warningText.text = "";  // 텍스트 초기화

        // 한 글자씩 출력
        for (int i = 0; i < message.Length; i++)
        {
            warningText.text += message[i];  // 한 글자씩 추가

            // 텍스트 효과음 재생
            if (textEffectAudioSource != null)
            {
                textEffectAudioSource.PlayOneShot(textEffectAudioSource.clip);  // 한 글자마다 효과음 재생
            }

            yield return new WaitForSeconds(0.1f);  // 한 글자 출력 후 0.1초 대기
        }
    }

    // 플레이어가 InputField에 이름을 입력하였는지 여부
    private void CheckNameInput(string inputText)
    {
        if (!string.IsNullOrEmpty(inputText))
        {
            isNameEntered = true;  // 이름이 입력되었음을 기록
            PlayerPrefs.SetString("PlayerName", inputText); // PlayerPrefs에 이름 저장
        }
        else
        {
            isNameEntered = false;  // 이름이 입력되지 않았음을 기록
        }
    }

    public IEnumerator OnInputField()
    {
        // 이름이 입력되었는지 확인
        if (isNameEntered)
        {
            // 이름이 입력된 경우, 버튼들을 위로 올리고 Text7과 Text8을 출력
            yield return StartCoroutine(MoveButtonUp());  // Start 버튼 위로
            yield return StartCoroutine(optionButtonController.MoveButtonUp());  // Option 버튼 위로
            yield return StartCoroutine(quitButtonController.MoveButtonUp());  // Quit 버튼 위로

            mainPlayerName.SetActive(false); // InputField 비활성화
            realOptionButton.SetActive(false); //BackButton 비활성화

            // Text7과 Text8을 순차적으로 출력
            yield return StartCoroutine(multiTextDisplayController.DisplayText(multiTextDisplayController.textObjects[6], multiTextDisplayController.messages[6]));
            yield return new WaitForSeconds(multiTextDisplayController.betweenTextDelay);
            multiTextDisplayController.textObjects[6].gameObject.SetActive(false);

            yield return StartCoroutine(multiTextDisplayController.DisplayText(multiTextDisplayController.textObjects[7], multiTextDisplayController.messages[7]));
            yield return new WaitForSeconds(multiTextDisplayController.betweenTextDelay);
            multiTextDisplayController.textObjects[7].gameObject.SetActive(false);

            // 다음 씬으로 이동
            SceneManager.LoadScene("CityScene"); // "CityScene"을 다음 씬의 이름으로 변경
        }

        if(!isNameEntered)
        {
            // 이름이 입력되지 않았을 경우 경고 메시지 출력
            StartCoroutine(DisplayWarningText7And8());
        }
    }

    // 경고 텍스트를 출력하는 코루틴
    private IEnumerator DisplayWarningText7And8()
    {
        isWarningActive = true;  // 경고 메시지가 출력 중임을 기록
        isStartButtonReturn = false; // 버튼 애니메이션이 끝남

        // 경고 메시지에 따른 텍스트 설정
        if (warningCount == 0)
        {
            errorText.text = "아까 내가 말했지? 이름 작성하라고!";

            errorText.gameObject.SetActive(true);  // 경고 텍스트 활성화

            // 경고 효과음 재생
            if (warningTextAudioSource != null)
            {
                warningTextAudioSource.PlayOneShot(warningTextAudioSource.clip);
            }

            // 경고 메시지 출력 후 대기 시간
            yield return new WaitForSeconds(2.0f);

            // 경고 텍스트 비활성화
            errorText.gameObject.SetActive(false);
        }
        else if (warningCount == 1)
        {
            errorText.text = "혹시 한국말 못 알아들어..??";

            errorText.gameObject.SetActive(true);  // 경고 텍스트 활성화

            // 경고 효과음 재생
            if (warningTextAudioSource != null)
            {
                warningTextAudioSource.PlayOneShot(warningTextAudioSource.clip);
            }

            // 경고 메시지 출력 후 대기 시간
            yield return new WaitForSeconds(2.0f);

            // 경고 텍스트 비활성화
            errorText.gameObject.SetActive(false);
        }
        else if (warningCount == 2)
        {
            errorText.text = "에휴.. 멍청한 놈..ㅉ 나가서 이름 적는것부터 배우고 와;;";

            // 경고 텍스트 활성화
            errorText.gameObject.SetActive(true);

            // 경고 효과음 재생
            if (warningTextAudioSource != null)
            {
                warningTextAudioSource.PlayOneShot(warningTextAudioSource.clip);
            }

            // 경고 메시지가 충분히 보일 수 있도록 대기
            yield return new WaitForSeconds(2.0f);

            // 경고 텍스트 비활성화
            errorText.gameObject.SetActive(false);

            // 마지막 경고 메시지 이후 게임 강제 종료
            Application.Quit();  // 게임 강제 종료
            yield break;
        }

        // 경고 횟수 증가
        warningCount++;

        isWarningActive = false;  // 경고 메시지 출력이 끝남을 기록
    }
}