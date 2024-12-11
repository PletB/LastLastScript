using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FieldFiveTrigger : MonoBehaviour
{
    // TextMeshPro �ؽ�Ʈ �� UI ���� ������Ʈ��
    public TMP_Text dialogueText; // ��� �ؽ�Ʈ ǥ�ÿ� TextMeshPro �ؽ�Ʈ
    public GameObject textBox; // �ؽ�Ʈ �ڽ� ������Ʈ
    public TMP_Text promptText; // "FŰ�� ��������" �ȳ� �ؽ�Ʈ
    public SPUM_Prefabs anim; // �÷��̾� �ִϸ��̼� ����
    public AudioSource textTrueEffect; // Ʈ���� ���� �� ����� ����
    public AudioSource textFalseEffect; // Ʈ���� ��� �� ����� ����
    public AudioSource textEffect; // �ؽ�Ʈ ��� �� ����� ����
    public GameObject MidleWall; // ����Ʈ ���� �� ���ŵ� �� ������Ʈ
    public GameObject SixTrigger; // ����Ʈ �Ϸ� �� Ȱ��ȭ�� Ʈ���� ������Ʈ
    public float textDisplayDelay = 0.05f; // �ؽ�Ʈ �� ���ھ� ��µ� ���� ������

    // ���� ���� ���� ������
    private bool isPlayerInTrigger = false; // �÷��̾ Ʈ���� ���� �ִ��� ����
    private bool isTextDisplaying = false; // �ؽ�Ʈ�� ��� ������ ����
    private int dialogueIndex = 0; // ���� ����� �ε���
    private bool questAccepted = false; // ����Ʈ ���� ����
    private bool questDeclined = false; // ����Ʈ ���� ����
    private bool questCompleted = false; // ����Ʈ �Ϸ� ����
    public int monstersCaptured = 0; // ������� ���� ���� ��
    public int requiredMonsters = 5; // ����Ʈ �Ϸῡ �ʿ��� ���� ��
    private QuestFieldPlayer playerScript; // �÷��̾� ��ũ��Ʈ ����

    // ��� �迭
    private readonly string[] dialogues = new string[]
    {
        "�� �ٽ� ���� �ݰ���?",
        "���� ������ �� �ؾ����� �˷��ٰ�.",
        "���� ���̴� ������ �ù� 5�� ��ƿ�",
        "�� ���ϰ� �ʸ��ʸ��� ���� ��.. �ʹ� �ҽ���..�Ф�",
        "�� �������� ���� ��ƿ��°ž�.",
        "�ٵ�.. ������ ���� ��´�?.. ����� �̾��ؼ� �� ����;;",
        "�� �ʰ� ��� ����� �װ� �� �˺� �ƴ�����..��",
        "��ư, �� ������ �ù� 5���� ��ƿ��⸸ �ϸ� ��.",
        "����Ʈ : ������ �ù� 5�� ��ƿ���",
        "�����Ͻðڽ��ϱ�?",
        "����;; ���� ���ҰŸ� �� �Ծ�..?", // "�ƴϿ�" ��ư Ŭ�� ��
        "�׳� ���� ������!",
        "����! ���� '5��' ��ƿ��� �� � ��!", // "��" ��ư Ŭ�� ��
        "���� ���Ҿ�;;",
        "���� ���Ҵٴϱ�?!",
        "���͸� �� �˾Ƶ�� ���� ���Ҵٰ�..",
        "��... �ȵǰڴ�.. ������ ���� �˷��ٰ�.. 30���� ��ƿ�;;",
        "��! ���ɵ� �� ��ҳ�. ��",
        "���� �� �ؾ��Ұ� �ִµ�.. �װ� ���� �˷��ֱ� �Ȱ�",
        "ALT �� F4 ���ÿ� ������.",
        "�׷� ���� ����Ʈ ����. ��¥��!��",
        "�����̶�ϱ�! ���� �Ȳ����� �ص׾�~",
        "�� �ϰ� �ѹ��� ������!",
        "��¥ ������ �ƽ���Ű� ������^^",
        "���� ����Ʈ�� �ް� ������ ���� ��Ż�� Ÿ�� ��.",
        "�´� �� ģ����.����",
        "���� �Ȱ���..? �� ����..;;"
    };

    public MoveCamera moveCamera; // MoveCamera ��ũ��Ʈ ����
    public Button acceptButton; // "��" ��ư
    public Button declineButton; // "�ƴϿ�" ��ư

    private void Start()
    {
        // �ؽ�Ʈ �ڽ��� ��ư �ʱ�ȭ
        if (textBox != null) textBox.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (SixTrigger != null) SixTrigger.SetActive(false); // SixTrigger ���� �� ��Ȱ��ȭ

        if (acceptButton != null) acceptButton.gameObject.SetActive(false);
        if (declineButton != null) declineButton.gameObject.SetActive(false);

        // ��ư Ŭ�� �̺�Ʈ ����
        acceptButton.onClick.AddListener(OnAccept);
        declineButton.onClick.AddListener(OnDecline);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �÷��̾ Ʈ���ſ� ������ �� ó��
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            // ù ��� ��� �� "FŰ�� ��������" �ؽ�Ʈ ǥ��
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
        // �÷��̾ Ʈ���Ÿ� ����� �� ó��
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            // "FŰ�� ��������" �ؽ�Ʈ �����
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
        // �÷��̾ FŰ�� ������ �� ��ȭ ����
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F) && !isTextDisplaying
            && !acceptButton.gameObject.activeSelf && !declineButton.gameObject.activeSelf)
        {
            if (questAccepted && !questCompleted && dialogueIndex >= 17)
            {
                return; // ����Ʈ ���� �߿��� ��ȭ ��ŵ
            }

            if (dialogueIndex < dialogues.Length)
            {
                if (questAccepted && dialogueIndex < 12)
                {
                    dialogueIndex = 12; // ���� �� ��ȭ ����
                }

                StartCoroutine(DisplayTextWithAnimation(dialogues[dialogueIndex]));

                if (questDeclined && dialogueIndex >= 11)
                {
                    Application.Quit(); // ����Ʈ ���� �� ���� ����
                    Debug.Log("������ �������");
                }

                dialogueIndex++;
            }
        }
    }

    public void MonsterCaptured()
    {
        // ���� óġ ī��Ʈ ������Ʈ
        if (questAccepted && !questCompleted)
        {
            monstersCaptured++;

            // Ư�� ��ȭ �ܰ迡�� �䱸 ���� �� ����
            if (dialogueIndex == 17)
            {
                requiredMonsters = 30;
            }

            // ����Ʈ �Ϸ� ���� Ȯ��
            if (monstersCaptured >= requiredMonsters)
            {
                questCompleted = true;
                StartCoroutine(CompleteQuest());
                dialogueIndex = 17; // ����Ʈ �Ϸ� �� ��ȭ ����
            }
        }
    }

    private IEnumerator DisplayTextWithAnimation(string message)
    {
        // �ؽ�Ʈ �ִϸ��̼� ���
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

        // Ư�� ��ȭ �ܰ迡�� ī�޶� �� ��
        if (dialogueIndex == 3)
        {
            moveCamera.ZoomInOnTarget();
        }

        // ��ȭ �Ϸ� �� ��ư ǥ��
        if (dialogueIndex == 10)
        {
            if (acceptButton != null) acceptButton.gameObject.SetActive(true);
            if (declineButton != null) declineButton.gameObject.SetActive(true);
        }

        isTextDisplaying = false;
    }

    private void OnAccept()
    {
        // ����Ʈ ���� ó��
        questAccepted = true;
        dialogueIndex = 12; // ��� ����
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        MidleWall.SetActive(false); // �� ����
    }

    private void OnDecline()
    {
        // ����Ʈ ���� ó��
        questDeclined = true;
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
    }

    public IEnumerator CompleteQuest()
    {
        // ����Ʈ �Ϸ� ó��
        yield return new WaitForSeconds(0);

        // ��� ���� ��Ȱ��ȭ
        MonsterController[] monsters = FindObjectsOfType<MonsterController>();
        foreach (MonsterController monster in monsters)
        {
            monster.DisableMonster();
        }

        // SixTrigger Ȱ��ȭ
        if (SixTrigger != null)
        {
            SixTrigger.SetActive(true);
        }
    }
}
