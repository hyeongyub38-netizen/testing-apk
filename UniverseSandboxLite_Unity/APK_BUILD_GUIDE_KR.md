# Android APK 빌드 가이드

이 프로젝트는 Windows exe뿐 아니라 Android APK로도 빌드할 수 있습니다.

## 1. Unity Hub에서 Android 모듈 설치

Unity Hub → Installs → 사용 중인 Unity 버전의 톱니바퀴 → Add modules에서 아래 항목을 설치하세요.

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

이 3개가 없으면 APK 빌드 버튼이 안 뜨거나 오류가 납니다.

## 2. 프로젝트 열기

1. Unity Hub 실행
2. Add project from disk
3. `UniverseSandboxLite_Unity` 폴더 선택
4. 상단 메뉴 `Universe Sandbox Lite > Create Demo Scene`
5. `Assets/Scenes/UniverseSandboxLite.unity` 열기

## 3. Android로 전환

1. `File > Build Settings...`
2. Platform에서 `Android` 선택
3. `Switch Platform` 클릭
4. `Add Open Scenes` 클릭
5. `Player Settings...` 클릭

추천 설정:

- Company Name: `HyeongyuLab`
- Product Name: `Universe Sandbox Lite`
- Package Name: `com.hyeongyulab.universesandboxlite`
- Orientation: `Landscape Left`
- Minimum API Level: Android 7.0 이상 권장

## 4. APK로 빌드

Build Settings에서:

- `Build App Bundle (Google Play)` 체크 해제 → APK로 빌드
- `Build` 클릭
- 저장 위치 선택
- `.apk` 파일 생성

## 5. 휴대폰에 설치

1. APK 파일을 휴대폰으로 옮기기
2. 설치할 때 “알 수 없는 앱 설치 허용”이 필요할 수 있음
3. 실행

## 모바일 조작법

- 왼쪽 아래 드래그: 이동
- 오른쪽 화면 드래그: 시점 회전
- 두 손가락 벌리기/좁히기: 줌 인/아웃
- 행성 터치: 선택
- 오른쪽 버튼: 소행성 발사, 블랙홀 생성, 포커스, 따라가기, 궤도 삭제
- 위쪽 버튼: 일시정지, 리셋, 시간 배속 조절, 모바일 성능 프리셋

## 렉 걸리면

- 우측 `Trail X`로 궤도 흔적 삭제
- 상단 `성능` 버튼 누르기
- 시간 배속을 너무 높이지 않기
- 블랙홀과 소행성을 너무 많이 만들지 않기

이건 모바일에서도 돌아가게 만든 스타터킷이라, 폰 성능에 따라 행성/파티클 개수를 줄이는 게 좋습니다.
