# 프로젝트 빠른 시작

한국 전통놀이 VR 체험 프로젝트다. Unity 6000.3.10f1을 사용한다. 작업 전 `ProjectSettings/ProjectVersion.txt`를 확인해 실제 Unity 버전을 기준으로 삼는다.

## 작업 전 확인

- 수정 전에 `git status`로 사용자 변경을 확인하고 보존한다. 요청 없이 reset, stash, discard, commit을 실행하지 않는다.
- 사방치기 작업은 `Assets/Script/사방치기규칙_구체화.txt`를 세부 사양의 기준으로 삼는다.
- `Assets/한국 전통놀이 VR 체험 기획서.pdf`는 전체 방향을 참고한다. 세부 규칙이나 사용자 요청과 충돌하면 사용자 요청, 구체화 규칙, 기획서 순으로 적용한다.
- 사방치기 핵심 코드는 `Assets/Script/Map.cs`, `Assets/Script/SabangStone.cs`, `Assets/Script/UIDoor.cs`이다.
- 관련 없는 씬, 에셋, 사용자 변경을 정리하거나 재작성하지 않는다.

## 작업 완료 기준

- 코드 변경은 관련 컴파일 또는 Unity Console 오류 확인을 수행한다.
- XR 입력, 물리, UI, Rigidbody, UnityEvent 변경은 컴파일 성공과 Unity Play Mode 확인을 구분해 보고한다.
- 검증할 수 없었던 동작, Inspector 재연결 필요 여부, 프로토타입 한계를 명시한다.
- 한국어 응답의 문장은 마침표, 물음표 또는 느낌표로 끝낸다.

`Assets/Script` 안의 C# 작업에는 해당 폴더의 `AGENTS.md`를 추가로 적용한다.
