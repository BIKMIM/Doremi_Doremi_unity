//지금 프로젝트에서 실제로 참조되고 동작을 책임지는 건 다음 스크립트들이에요:

//StaffLineRenderer(오선 생성)

//NoteDataLoader, SongList (JSON → 객체)

//NotePrefabProvider (음표·쉼표 프리팹 제공)

//LedgerLineHelper (보조선 생성)

//NoteMapper (음표 이름 → lineIndex 매핑)

//NoteSpawner (위 전부 조합해서 악보 렌더링)

//PlayNote (버튼 클릭 시 소리 재생)

//반면, 아래 두 개는 현재 어디에서도 호출되지 않아서 “필요 없는” 상태입니다:

//NoteMapping

//반음 단위로 동적으로 lineIndex 를 계산해 주는 유틸이지만, 실제 렌더러(NoteSpawner)에선 고정 매핑 테이블(NoteMapper)만 쓰고 있어요.

//NotePlacer

//에디터나 단일 노트 테스트용으로 만든, lineIndex 슬라이더로 음표 한 장만 옮겨 보는 스크립트인데, 자동 배치를 모두 NoteSpawner로 대체했으니 더 이상 사용되지 않습니다.

//⚙️ 만약 반음 계산 방식이 더 마음에 들면 NoteMapper 를 없애고 NoteMapping 을 연결해도 되고,
//에디터 상 테스트가 필요 없다면 NotePlacer 도 지워주시면 프로젝트가 좀 더 깔끔해질 거예요.





//1. 에디터 모드(씬 편집 시)
//StaffLineRenderer(OnValidate)

//인스펙터에서 값이 변경되면 OnValidate()가 호출

//EditorApplication.update에 DelayedRedraw()를 한 번만 등록

//다음 에디터 프레임에서 DelayedRedraw() →

//기존 자식(이전 그려진 선) 삭제(ClearChildren())

//DrawStaffLines() 실행 → 5줄의 오선을 linesContainer에 그려 줌

//이렇게 해 두면 씬 뷰 상에서 실시간으로 오선 높이(staffHeight, lineThickness)를 조정해 볼 수 있습니다.

//2. 플레이 모드(게임 실행 시)
//StaffLineRenderer(Start)

//Application.isPlaying == true 이므로

//에디터 모드와 동일하게 ClearChildren() → DrawStaffLines() 실행

//linesContainer에 5줄 오선을 배치

//NoteSpawner(Awake → Start)

//Awake()

//NoteDataLoader 생성자에 songsJson 주입 → JSON 로더 준비

//NoteMapper 인스턴스 생성 → 음이름 → lineIndex 매핑 준비

//LedgerLineHelper 생성자에 보조선 프리팹과 notesContainer 전달 → 보조선 헬퍼 준비

//Start()

//ClearNotes()

//notesContainer 자식(이전 음표들) 모두 삭제

//SpawnSongNotes()

//dataLoader.LoadSongs() 호출 → SongList 객체 반환

//selectedSongIndex에 해당하는 Song 선택 → notes 문자열 배열 획득

//가로 위치 초기화

//전체 음표 간격 = beatSpacing * (count-1)

//currentX = -totalSpan/2 (화면 중앙 정렬용)

//기준선(baseline) 계산

//linesContainer.GetChild(2) → 3번째 오선(RectTransform) Y 좌표 → baselineY

//세로 간격 = staffHeight / 4

//각 음표 토큰(token)마다:

//token.Split(':') → pitch(예 "C4")와 code(예 "4R") 분리, isRest 판별

//높이 인덱스 계산

//noteMapper.TryGetIndex(pitch, out index) → 사전의 고정값(예: "E4" → -3)

//stemDown = index >= -1

//프리팹 선택

//prefabProvider.GetPrefab(code, stemDown) → 해당 음표/쉼표 프리팹 반환

//인스턴스화 & 배치

//Instantiate(prefab, notesContainer)

//RectTransform 설정(앵커·피벗·스케일·회전)

//Y = baselineY + index * spacing + noteYOffset

//X = currentX

//보조선(ledger) 생성

//GenerateLedgerLines(index, baselineY, spacing, currentX, ledgerYOffset)

//index 값이 오선 범위를 벗어나면(예: C4이하, G5이상) 보조선 프리팹 인스턴스화

//다음 음표 X 위치 갱신

//currentX += beatSpacing * GetBeatLength(code)

//결과:

//linesContainer에는 5줄 오선 + 필요하면 위아래 보조선

//notesContainer에는 JSON에 정의된 순서대로 음표(또는 쉼표) 아이콘들이 중앙 정렬되어 배치

//3. 버튼 클릭 시 음 재생
//씬 내 Piano 키 버튼들에는 PlayNote 컴포넌트가 붙어 있고,
//각 버튼의 OnClick 혹은 IPointerDown 이벤트에 PlayC4(), PlayD4() 같은 메서드를 연결

//PlayNote.OnPointerDown(또는 OnClick) →

//Debug.Log로 어떤 음인지 찍고

//audioSource.PlayOneShot(clickSound)으로 해당 음원 재생

//요약
//에디터 : StaffLineRenderer → 오선 그리기

//플레이 실행

//StaffLineRenderer → 오선(5줄) 그리기

//NoteSpawner

//JSON 로드 → SongList

//악보 파싱 → noteMapper로 lineIndex 계산

//NotePrefabProvider로 음표·쉼표 프리팹 가져오기

//노트 인스턴스화 & LedgerLineHelper로 보조선 그리기

//UI 인터랙션 : PlayNote → 버튼 누르면 사운드 재생

//이 흐름이 현재의 “자동 악보 렌더링 및 재생”의 전 과정을 담고 있습니다. 추가로 궁금한 부분 있으면 언제든 말씀해 주세요!