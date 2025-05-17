using UnityEngine;
using UnityEngine.UI;

public class StaffLineDrawer : MonoBehaviour
{
    [Header("오선을 그릴 대상 패널")]  // 헤더는 제목. 헤더 아래에 변수 선언.
    public RectTransform staffPanel;  // 패널 변수 선언.

    [Header("줄 프리팹")]  // 헤더는 제목. 헤더 아래에 변수 선언.
    public GameObject linePrefab;  // 프리팹 변수 선언.

private void Start()
    {
        DrawStafflLines();  // 시작할 때 한 줄 그리기.
    }

 private void DrawStafflLines()  // 한 줄 그리기.
    {
        if (staffPanel == null || linePrefab == null)  // 패널이나 프리팹이 설정되지 않았다면
        {
            Debug.LogError("StaffPanel 또는 LinePrefab이 설정되지 않았습니다!");  // 에러 메시지 출력.
            return;  // 종료.   
        }

        float staffPanelHeight = staffPanel.rect.height;  // 패널의 높이를 가져옴.
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);  // 각 줄 사이의 간격을 계산.
        float thickness = MusicLayoutConfig.GetLineThickness(staffPanel);  // 줄의 두께를 계산해서 Math함수로 묶은다음에 spacing과 곱해서 줄의 두께를 계산. 혹시 해상도가 높아서 너무 얇게 보이면 최소값인 2F로 할 것.


        for (int i = -2; i <= 2; i++)  // 총 5줄. -2, -1, 0, 1, 2.
        {
            GameObject line = Instantiate(linePrefab, staffPanel);  // 프리팹을 인스턴스화하여 패널에 추가.
            RectTransform lineRt = line.GetComponent<RectTransform>();  // 인스턴스화된 객체의 RectTransform 컴포넌트를 가져옴.

            lineRt.anchorMin = new Vector2(0f, 0.5f);  // 줄의 앵커를 설정.    
            lineRt.anchorMax = new Vector2(1f, 0.5f);  // 줄의 앵커를 설정.
            lineRt.pivot = new Vector2(0.5f, 0.5f);  // 줄의 피벗을 설정.  
            lineRt.sizeDelta = new Vector2(0, thickness);  // 줄의 크기를  설정.


            int y = Mathf.RoundToInt(i * spacing); // 줄의 위치를 소숫점이 아니라 정수화해서 픽셀경계에 닿아서 선이 안티얼라이싱때문에 두꺼워지는 것 막기 위한 코드
            lineRt.anchoredPosition = new Vector2(0, y);  // 줄의 위치를 설정.바로 윗줄과 연관해서 선이 일부 두꺼워지는걸 막기 위한 코드
        }
    }

}
