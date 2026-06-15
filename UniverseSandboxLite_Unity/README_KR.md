# Universe Sandbox Lite - Unity Starter Kit

현규가 말한 **Universe Sandbox 비슷한 Windows exe 게임**을 만들기 위한 Unity 스타터킷입니다.
바로 상업게임급은 아니지만, HTML보다 훨씬 확장하기 좋은 뼈대입니다.

## 들어있는 기능

- 태양계 비슷한 3D 공간
- 태양, 수성, 금성, 지구, 달, 화성, 목성, 토성, 천왕성, 해왕성 자동 생성
- 중력 기반 공전 시뮬레이션
- 행성 충돌 / 합체
- 지구로 유도되는 소행성 발사
- 블랙홀 추가 및 흡수
- 궤도 Trail 표시
- 행성 선택 UI
- 시간 배속 / 중력 조절
- 카메라 자유 이동
- Unity에서 Windows exe 빌드 가능
- Android APK 빌드 가능
- 모바일 터치 조작 지원
- 모바일 전용 큰 버튼 UI
- 모바일 성능 프리셋

## 사용법

### 방법 A: 이 폴더를 Unity Hub에서 바로 열기

1. Unity Hub 실행
2. `Add project from disk` 선택
3. 이 폴더 `UniverseSandboxLite_Unity` 선택
4. 프로젝트가 열리면 상단 메뉴에서:
   `Universe Sandbox Lite > Create Demo Scene`
5. `Assets/Scenes/UniverseSandboxLite.unity` 씬 열기
6. ▶ Play 누르기

### 방법 B: 기존 Unity 프로젝트에 넣기

1. Unity에서 새 3D 프로젝트 생성
2. 이 압축파일 안의 `Assets/Scripts`, `Assets/Editor` 폴더를 프로젝트의 `Assets`에 복사
3. Unity 상단 메뉴에서:
   `Universe Sandbox Lite > Create Demo Scene`
4. 생성된 씬을 열고 ▶ Play

## 조작법

- `W A S D`: 이동
- `Q / E`: 아래 / 위 이동
- 마우스 우클릭 드래그: 시점 회전
- 마우스 휠: 전진/후진
- 좌클릭: 행성 선택
- 더블클릭: 선택한 행성으로 이동
- `F`: 선택한 행성 포커스
- `V`: 선택한 행성 따라가기 on/off
- `Space`: 일시정지
- `L`: 지구로 소행성 발사
- `B`: 블랙홀 추가
- `R`: 태양계 리셋
- `T`: 궤도 흔적 지우기
- `1 / 2`: 시간 배속 낮추기 / 높이기

## 모바일 조작법 / APK 기능

APK로 빌드하면 자동으로 모바일 HUD가 켜집니다.

- 왼쪽 아래 드래그: 이동
- 오른쪽 화면 드래그: 시점 회전
- 두 손가락 벌리기/좁히기: 줌 인/아웃
- 행성 터치: 선택
- 오른쪽 버튼: 소행성 발사, 블랙홀 생성, 포커스, 따라가기, 궤도 삭제
- 위쪽 버튼: 일시정지, 리셋, 시간 배속 조절, 성능 프리셋

자세한 APK 빌드법은 `APK_BUILD_GUIDE_KR.md`를 보세요.

## Windows exe로 뽑는 법

1. Unity 상단 메뉴 `File > Build Settings...`
2. Platform을 `Windows, Mac, Linux`로 선택
3. Target Platform: `Windows`
4. Architecture: `x86_64`
5. `Add Open Scenes` 클릭
6. `Build` 클릭
7. 폴더 선택하면 `.exe` 생성됨

## Android APK로 뽑는 법

1. Unity Hub에서 현재 Unity 버전에 `Android Build Support`, `Android SDK & NDK Tools`, `OpenJDK` 설치
2. Unity 상단 메뉴 `File > Build Settings...`
3. Platform에서 `Android` 선택
4. `Switch Platform` 클릭
5. `Add Open Scenes` 클릭
6. `Build App Bundle (Google Play)` 체크 해제
7. `Build` 클릭
8. `.apk` 파일 생성

추천 설정은 `Player Settings`에서:

- Orientation: Landscape Left
- Package Name: `com.hyeongyulab.universesandboxlite`
- Minimum API Level: Android 7.0 이상

## 다음 업그레이드 추천

1. 실제 행성 텍스처 넣기
2. 폭발 파편을 진짜 조각 메시로 만들기
3. 행성 생성 메뉴 추가
4. 소행성 여러 개 동시 발사
5. 블랙홀 주변 렌즈 효과 셰이더 추가
6. 저장/불러오기 추가
7. 그래픽 옵션 메뉴 추가
8. 모바일용 행성 생성 탭 추가
9. 터치 조이스틱 이미지를 진짜 UI 이미지로 교체
10. APK 아이콘 / 로딩 화면 만들기

## 주의

이건 “Universe Sandbox Lite” 느낌의 시작 버전입니다.  
진짜 Universe Sandbox처럼 과학적으로 완벽한 시뮬레이터는 아니고, 게임처럼 보기 좋게 스케일을 조정한 버전입니다.
