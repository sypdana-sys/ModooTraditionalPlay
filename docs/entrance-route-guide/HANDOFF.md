# 전통마을 입구 길 안내 — 현재 상태와 다음 작업 인계

최종 갱신: 2026-09-04 (KST). 프로젝트: `D:/work/ModooTraditionalPlay`.
이 문서는 최신 상태 기준이며, 과거 작업 경위는 [WORKLOG.md](WORKLOG.md)를 참고한다. 과거 로그의 Unlit 적용 기록은 아래 Lit 점검 결과로 대체된다.

## 사용자 확정 지침

- Unity MCP로 실제 Editor 상태를 확인하고 한국어로 짧게 보고한 뒤 수정한다. 경로·Transform·방향을 추측하지 않는다.
- 유일한 대상 Scene은 `Assets/Scenes/main_playoursound.unity`. 장구·사방치기 Scene은 열거나 저장하지 않는다.
- XR Rig Transform, XR 이동/Interaction, Terrain, KHS 환경, 건축물, 전체 Lighting, 기존 Manager와 게임 로직을 보존한다.
- **별도 Scene 백업 파일을 만들지 않는다.** 사용자는 Git 커밋으로 백업과 이력을 관리할 예정이다. 현재 커밋·푸시는 실행하지 않았다.
- **Unlit 적용이나 조명 반응을 바꾸는 우회 조치는 사전에 사용자에게 이유와 영향을 설명하고 확인받는다.** 사용자가 직접 수정한 Lit 상태를 유지한다.
- 잔치 중인 마을에서 잔치하는 집으로 초대받는 분위기. 한국적인 깃발·천막·청사초롱·잔칫상으로 길을 유도한다. 스토리는 미확정이며 추후 요청에 맞춰 조정한다.
- 현재 구현 범위는 입구 첫 구간이다. 문까지의 전체 길이나 분기 구간을 임의로 확장하지 않는다.
- 신규 패키지·런타임 스크립트·안내 프레임워크·외부 에셋 추가 금지. 예외로 사용자가 승인한 Noto Sans KR Bold 글꼴은 이미 추가했다.

## 확인된 프로젝트와 목적지

- Unity `6000.3.10f1`, XR Interaction Toolkit `3.3.1`.
- XR Rig: `XR Origin Hands (XR Rig)`.
- 카메라: `XR Origin Hands (XR Rig)/Camera Offset/Main Camera`.
- 기존 이동 구조: LocomotionMediator/XRBodyTransformer, DynamicMoveProvider, 회전·텔레포트·중력·점프·등반 관련 컴포넌트. 설정 변경 없음.
- Terrain: `AllLevel/Terrain`, Terrain 및 TerrainCollider 활성, sharedMaterial 없음.
- Terrain Asset: `Assets/Naganeupseong/Scene/Resource/Terrain.asset`.

| 실제 게임 시작 문 | 게임 | 확인한 World Position |
|---|---|---|
| `AllLevel/Door01k (3)` | **장구** | `(466.2, 10.36167, 664.685)` |
| `AllLevel/Door01k (7)` | **사방치기** | `(488.722, 10.04, 663.085)` |

문 번호는 사용자 정정 반영 결과다. 입구 안내판에서는 게임별 분기를 안내하지 않는다.

## XR 기준점 — 변경 금지

작업 전후 동일한 값:

| 항목 | 값 |
|---|---|
| Position | `(450.5939, 9.55, 709.6385)` |
| Rotation Euler | `(1.30743086, 148.6813, -0.000296923739)` |
| Scale | `(1, 1, 1)` |
| Rotation Quaternion | `(-0.003077056957408786, -0.962820827960968, 0.010986467823386193, -0.26989975571632388)` |
| Camera forward | `(0.5196626, -0.0228169933, -0.854066849)` |

표의 반올림 값으로 Transform을 다시 대입하지 않는다. 다음 작업 시작 시 실제 직렬화 값을 읽고 전후 비교한다. 기존 Euler hint와 표시 Euler가 다를 수 있다.

## 구현 상태

```text
RouteGuide
└─ Entrance
   ├─ EntranceGuideSign
   │  ├─ WoodenFace_ReusedKitchenBoard
   │  ├─ Post_L / Post_R
   │  └─ VillageTitle / DestinationArrow
   ├─ EntranceFlag_L
   ├─ EntranceFlag_R
   ├─ Barrier_R
   │  └─ RiceStraw_1..3
   └─ FeastCanopy
      ├─ ClothRoof_Placeholder / TimberPost들
      ├─ IndigoValance / RedBinding / RidgePole
      ├─ CanopyLantern_L / CanopyLantern_R
      └─ FeastStrawMat / 소반 2개 / 그릇 2개
```

깃발에는 오방색 리본과 청사초롱 장식이 있다. 청사초롱은 총 4개이며 Light를 추가하지 않았다. 깃발·천막·등은 단순 제작 형태로, 최종 미술 에셋 교체 후보다.
`Barrier_L`, `CommonRoute`, `GameAreaJunction`, `JangguRoute`, `SabangchigiRoute`는 생성하지 않았다.

| 오브젝트 | World Position | Y 회전 | Scale |
|---|---|---|---|
| EntranceGuideSign | `(450.92746, 8.710405, 705.627441)` | `148.6813` | `(1,1,1)` |
| EntranceFlag_L | `(455.298767, 9.043716, 705.946045)` | `148.6813` | `(1,1,1)` |
| EntranceFlag_R | `(452.308746, 8.824467, 704.1268)` | `148.6813` | `(1,1,1)` |
| Barrier_R | `(450.117462, 8.656576, 703.4958)` | `148.6813` | `(1,1,1)` |
| FeastCanopy | `(450.7855, 8.8494, 700.8588)` | `148.6813` | `(1,1,1)` |

- 안내 문구: `전통놀이 체험마을` / `체험장 ↑`.
- 안내면 약 1.90 × 0.95m, 중심 높이 로컬 1.6m, 시작점과 수평거리 약 4.02m.
- 깃발 기둥 사이 3.50m, 천 사이 약 2.54m.
- 가이드 Collider 총 14개: 안내면 1, 안내판 기둥 2, 깃발 기둥 2, 짚단 3, 천막 기둥 4, 소반 2. 천·등에 Collider 없음.

## 재질 점검 — 2026-09-04

아래 경로는 모두 `Assets/RouteGuide/Entrance/` 기준이다. 추가한 일반 `.mat` 6개 및 실제 Renderer 연결을 Unity MCP로 확인했다. **6개 모두 `Universal Render Pipeline/Lit`, Emission 비활성**이다.

| 재질 파일 | 현재 상태 |
|---|---|
| Entrance_Indigo_Cloth.mat | Lit |
| Entrance_Ivory_Trim.mat | Lit |
| Festival_Canopy_Cloth.mat | 사용자가 직접 Lit으로 변경한 상태를 저장 |
| Festival_Dark_Wood.mat | Lit |
| Festival_Red_Cloth.mat | Lit |
| Festival_Unbleached_Cloth.mat | Lit |

글자는 `TextMeshPro/Mobile/Distance Field` 전용 셰이더를 유지한다. 이를 일반 장식 재질로 간주해 Lit으로 일괄 변환하지 않는다. 기존 목재 `Assets/Naganeupseong/Resource/Materials/M_Kitchen_Board.mat`도 기존 셰이더를 유지한다.

현재 Lit 상태의 시작 카메라 이미지:

![Lit 상태의 입구 안내](images/20260904-lit-current.png)

## 사용 에셋과 변경 파일

기존 재사용 Prefab은 `Assets/Naganeupseong/Prefabs/Prop/` 아래:

- `Kitchen_Board.prefab`, `Rice_Straw.prefab`
- `Small_Dining_Table.prefab`, `Straw_Mat01a.prefab`, `Bibimbap01a.prefab`

작업 변경 경로:

- `Assets/Scenes/main_playoursound.unity`
- `Assets/RouteGuide/Entrance/`의 위 재질 6개와 각 `.meta`
- 같은 폴더의 `Entrance_Swallowtail_Placeholder.asset`, `Festival_Hanging_Ribbon.asset`, `Festival_Cloth_Canopy.asset` 및 `.meta`
- `Assets/RouteGuide/Entrance/Fonts/NotoSansKR-Bold.otf`, `OFL.txt`, `Entrance_NotoSansKR_Bold_SDF.asset` 및 `.meta`
- 신규 폴더에 대응하는 `.meta`
- `docs/entrance-route-guide/HANDOFF.md`, `WORKLOG.md`, `images/20260904-lit-current.png`

글꼴은 Noto Sans KR Bold, SIL Open Font License. TMP SDF는 현재 문구용 정적 글리프이므로 문구 변경 시 한글·화살표 누락을 확인한다. 공식 출처: https://github.com/notofonts/noto-cjk/blob/main/Sans/SubsetOTF/KR/NotoSansKR-Bold.otf

## 검증과 남은 한계

- 기존 배치 작업에서 Scene View와 Game View 확인, 원본 Scene 저장·재열기 확인.
- XR Transform, 기존 환경·이동 설정 보존. 다른 게임 Scene 및 Build Settings 해시 유지 확인.
- 입구 전방 8m를 0.25m 간격 33지점, 반지름 0.3m/높이 1.8m 캡슐로 검사해 장애물 0 확인.
- 이번 Lit 점검에서 추가 일반 재질 6개 확인 및 사용자 변경 저장, 시작 카메라 이미지 갱신.
- Console Error 0 / Warning 0 확인. Console을 지우지 않았다.
- **XR HMD 실기 검증 필요**: 실제 VR 가독성, 2~3초 인지, 이동·텔레포트, 플레이어 높이별 시야, 천막 아래 음영과 재질 느낌. 정적 Editor 검사를 실기 완료로 간주하지 않는다.

## 다음 세션과 문서 관리

1. 이 문서와 WORKLOG의 최신 항목, Git 변경 목록을 읽는다.
2. Unity MCP로 연결 인스턴스, Unity 버전, 열린 Scene과 미저장 상태, XR 기준점, 실제 Hierarchy·목적지·TerrainCollider·재질·Console을 재확인한다. 불명확한 다른 Scene의 미저장 변경이 있으면 수정하지 않는다.
3. 사용자 직접 변경을 보존하고, 새 요청의 범위를 확인한 뒤 필요한 부분만 작업한다. 별도 백업 파일을 생성하지 않는다.
4. 스토리 확정 후 초대 문구·문양·잔칫상·천막을 다듬는다. 입구 이후 두 문까지의 유도는 추가 요청 시 진행한다.
5. 관련 작업마다 HANDOFF를 최신 상태로 갱신하고 WORKLOG에 날짜·변경 파일·검증·미검증을 추가한다. 재질 변경 시 현재 이미지도 갱신한다.

기존 외부 로그 `D:/Codex/Log/20260904_전통마을입구길안내_Log.md` 내용은 WORKLOG에 옮겨 두었으며 원본은 보존했다. 이후 인계용 최신 상태와 이력은 이 저장소의 두 MD에서 관리한다. 로그 삭제는 사용자 확인 없이 하지 않는다. Git 커밋·푸시는 별도 실행 여부를 명확히 보고한다.

다음 세션 시작용 문구:

> docs/entrance-route-guide/HANDOFF.md와 WORKLOG.md를 읽고 Unity MCP로 현재 상태부터 확인해줘. main_playoursound 입구 안내 작업을 이어가되 XR과 기존 환경을 보존하고 별도 백업 파일은 만들지 마. 추가 일반 재질은 현재 Lit이며 Unlit 적용은 먼저 내 확인을 받아야 해. 목적지는 Door01k (3) 장구, Door01k (7) 사방치기야. 실제 수정 전 확인 결과와 이번 요청에 해당하는 작업 범위를 짧게 보고해줘.
