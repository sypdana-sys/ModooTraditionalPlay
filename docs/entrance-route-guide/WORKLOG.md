# 2026-09-04 전통마을 입구 길 안내 작업 로그

## 작업 범위와 사용자 결정
- Unity MCP 직접 조회 및 수정. Unity 6000.3.10f1, 프로젝트 D:\work\ModooTraditionalPlay.
- 대상 Scene: D:\work\ModooTraditionalPlay\Assets\Scenes\main_playoursound.unity.
- 최초/최종 활성 Scene은 원본 main_playoursound 하나이며 저장 후 재열기 검증 완료.
- 사용자 정정: AllLevel/Door01k (7) = 사방치기, (488.722, 10.04, 663.085).
- 사용자 정정: AllLevel/Door01k (3) = 장구, (466.2, 10.36167, 664.685).
- 입구 첫 구간만 구현. 두 게임 개별 안내/문 상호작용/공통 경로 및 분기점은 구현하지 않음.
- 한글 폰트 Noto Sans KR Bold 및 TMP 에셋 추가 사용자 승인.
- 작업 도중 사용자 요청으로 별도 백업 파일 생성 중단. 직전 생성된 main_playoursound_20260904_150138_Backup.unity 및 meta는 제거 완료. 원본과 동일 SHA256을 확인한 뒤 제거했으며 활성 Scene/Build Settings로 사용하지 않았음.
- 현재 작업은 Git 커밋 전 검토 가능한 변경 상태. 이 실행에서 commit/push는 수행하지 않음.

## 작업 전 필수 확인
- Scene 경로 존재 확인, 최초 미저장 변경 없음, 타 Scene 열리지 않음.
- XR 경로: XR Origin Hands (XR Rig).
- Camera 경로: XR Origin Hands (XR Rig)/Camera Offset/Main Camera.
- 카메라 전방: (0.5196626, -0.0228169933, -0.854066849).
- XR Interaction Toolkit 3.3.1, uGUI 2.0.0.
- LocomotionMediator, XRBodyTransformer, DynamicMoveProvider, SnapTurnProvider/ContinuousTurnProvider, TeleportationProvider, GravityProvider, JumpProvider, ClimbProvider 구성 확인. Grab Move 오브젝트 비활성.
- 기존 XR Rig 하위 World Space Canvas 9개 및 TMP 사용 가능.
- 기존 TMP 4개는 필요한 한글 글리프 없음. 신규 승인 폰트에서 안내판 문자 11종 정적 SDF 생성, 저장/재열기 후 누락 0.
- Terrain: AllLevel/Terrain. Terrain 및 TerrainCollider 활성. Terrain 데이터 Assets/Naganeupseong/Scene/Resource/Terrain.asset.
- 시작 지형 높이 약 8.6584m. 원본 XR Rig y=9.55는 그대로 보존.
- 입구에서 Ox_Mill 옆의 열린 길을 따라 +X/-Z 방향으로 출발. 사용자 지정 두 문도 +X/-Z 방향의 마을 안쪽에 위치. 첫 8m만 통과 검사/안내 대상.
- 비진행 방향은 진행로 오른쪽 열린 공간. 소량 짚단으로 시각적 경계 제공. 완전 차단벽 없음.
- 재사용 표지판/깃발/천막/목책/새끼줄/화살표 전용 프리팹은 찾지 못함. 전통 문양 추가하지 않음.
- Console 작업 전 Error 0.

## XR Rig 작업 전후
| 항목 | 작업 전 | 작업 후 |
| --- | --- | --- |
| Position | 450.5939, 9.55, 709.6385 | 450.5939, 9.55, 709.6385 |
| Rotation (Euler) | 1.30743086, 148.6813, -0.000296923739 | 1.30743086, 148.6813, -0.000296923739 |
| Scale | 1, 1, 1 | 1, 1, 1 |
| Quaternion | -0.003077056957408786, -0.962820827960968, 0.010986467823386193, -0.26989975571632388 | 동일 |
- 최종 재열기 후 직렬화 Transform이 작업 전과 동일.
- 저장 전 XR 전체 컴포넌트 SHA256: 28C64A3BC0EB8D625D53B611667E84AE614374EFA860A150D3DE42F497F58E1D (전후 동일).
- 환경 및 Light 컴포넌트 해시도 배치 전후 동일. 최종 Git diff에서 기존 Scene 데이터 삭제/교체 없이 새 오브젝트와 SceneRoots 참조만 추가됨.
- TerrainCollider 조회 도구가 material getter를 읽으며 만든 임시 PhysicsMaterial 참조를 저장 비교 중 발견. 원본 Git 데이터의 null로 복원하고 재저장. 최종 TerrainCollider 변경 없음.

## 사용한 기존 에셋
- Assets/Naganeupseong/Prefabs/Prop/Kitchen_Board.prefab: 목재 안내면. 내부 모델 축을 확인해 표면이 시작 카메라를 향하도록 재배치.
- Assets/Naganeupseong/Resource/Materials/M_Kitchen_Board.mat: 기존 목재 재질 재사용. 원본 재질 변경 없음.
- Assets/Naganeupseong/Prefabs/Prop/Rice_Straw.prefab: Barrier_R의 짚단 3개.
- 검토한 대체 소품: Assets/Naganeupseong/Prefabs/Prop/Cloth_Roller.prefab, Jar01a.prefab, Cart.prefab. 배치하지 않음.
- KHS 원본 에셋 재Import/재다운로드 및 전체 환경 재배치 없음.

## 신규 에셋
- Assets/RouteGuide/Entrance/Fonts/NotoSansKR-Bold.otf
- Assets/RouteGuide/Entrance/Fonts/OFL.txt
- Assets/RouteGuide/Entrance/Fonts/Entrance_NotoSansKR_Bold_SDF.asset (텍스처/재질 서브에셋 포함)
- Assets/RouteGuide/Entrance/Entrance_Indigo_Cloth.mat
- Assets/RouteGuide/Entrance/Entrance_Ivory_Trim.mat
- Assets/RouteGuide/Entrance/Entrance_Swallowtail_Placeholder.asset
- 위 파일 및 Assets/RouteGuide, Entrance, Fonts 폴더의 Unity .meta.
- 폰트 출처: https://github.com/notofonts/noto-cjk/blob/main/Sans/SubsetOTF/KR/NotoSansKR-Bold.otf
- 라이선스 출처: https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE
- 깃발은 단순 제비꼬리형 Placeholder. 전통 깃발 최종 모델로 교체 검토 필요.
- 새 런타임 스크립트, Manager, ScriptableObject, 패키지 추가 없음.

## 생성한 Hierarchy
RouteGuide/Entrance 아래 EntranceGuideSign, EntranceFlag_L, EntranceFlag_R, Barrier_R.
Barrier_L은 불필요하여 미생성.
CommonRoute, GameAreaJunction, JangguRoute, SabangchigiRoute 미생성.

## 실제 Transform
Position/Rotation은 월드 좌표, Scale은 로컬값.
| 오브젝트 | Position | Rotation | Scale |
| --- | --- | --- | --- |
| RouteGuide | 0, 0, 0 | 0, 0, 0 | 1, 1, 1 |
| RouteGuide/Entrance | 0, 0, 0 | 0, 0, 0 | 1, 1, 1 |
| Entrance/EntranceGuideSign | 450.92746, 8.710405, 705.627441 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceGuideSign/WoodenFace_ReusedKitchenBoard | 450.92746, 10.3104057, 705.627441 | 0, 238.6813, 270 | 2.93, 1.27, 3.02 |
| WoodenFace_ReusedKitchenBoard/SM_Kitchen_Board | 450.92746, 10.3104057, 705.627441 | 0, 238.6813, 270 | 1, 1, 1 |
| EntranceGuideSign/Post_L | 451.589325, 9.590405, 705.9248 | 0, 148.6813, 0 | 0.1, 1.76, 0.1 |
| EntranceGuideSign/Post_R | 450.359161, 9.590405, 705.176331 | 0, 148.6813, 0 | 0.1, 1.76, 0.1 |
| EntranceGuideSign/VillageTitle | 450.87027, 10.530405, 705.721436 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceGuideSign/DestinationArrow | 450.87027, 10.1304054, 705.721436 | 0, 148.6813, 0 | 1, 1, 1 |
| Entrance/EntranceFlag_L | 455.298767, 9.043716, 705.946045 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceFlag_L/WoodenPole | 455.298767, 10.523716, 705.946045 | 0, 148.6813, 0 | 0.065, 2.96, 0.065 |
| EntranceFlag_L/Crossbar | 455.1023, 11.8037167, 705.8265 | 0, 148.6813, 0 | 0.6, 0.045, 0.045 |
| EntranceFlag_L/Cloth_Placeholder | 455.288361, 11.773716, 705.963135 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceFlag_L/IvoryHeader | 455.08075, 11.733717, 705.842651 | 0, 148.6813, 0 | 0.48, 0.07, 0.014 |
| Entrance/EntranceFlag_R | 452.308746, 8.824467, 704.1268 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceFlag_R/WoodenPole | 452.308746, 10.3044662, 704.1268 | 0, 148.6813, 0 | 0.065, 2.96, 0.065 |
| EntranceFlag_R/Crossbar | 452.505219, 11.5844669, 704.246338 | 0, 148.6813, 0 | 0.6, 0.045, 0.045 |
| EntranceFlag_R/Cloth_Placeholder | 452.7084, 11.5544662, 704.3934 | 0, 148.6813, 0 | 1, 1, 1 |
| EntranceFlag_R/IvoryHeader | 452.5008, 11.5144672, 704.2729 | 0, 148.6813, 0 | 0.48, 0.07, 0.014 |
| Entrance/Barrier_R | 450.117462, 8.656576, 703.4958 | 0, 148.6813, 0 | 1, 1, 1 |
| Barrier_R/RiceStraw_1 | 450.510437, 8.681967, 703.7349 | 0, 148.6813, 0 | 1, 1, 1 |
| RiceStraw_1/SM_Rice_Straw | 450.510437, 8.681967, 703.7349 | 0, 148.6813, 0 | 1, 1, 1 |
| Barrier_R/RiceStraw_2 | 450.17984, 8.661923, 703.39325 | 0, 195.6813, 0 | 1, 1, 1 |
| RiceStraw_2/SM_Rice_Straw | 450.17984, 8.661923, 703.39325 | 0, 195.6813, 0 | 1, 1, 1 |
| Barrier_R/RiceStraw_3 | 449.7245, 8.631186, 703.256653 | 0, 242.6813, 0 | 1, 1, 1 |
| RiceStraw_3/SM_Rice_Straw | 449.7245, 8.631186, 703.256653 | 0, 242.6813, 0 | 1, 1, 1 |

## 안내판과 Collider
- 안내판 목재면 약 1.90m x 0.95m. 지면 기준 중심 높이 1.60m.
- 표지판 뿌리와 XR Rig 수평 거리 약 4.02m.
- 문구: 전통놀이 체험마을 / 체험장 ↑.
- 제목 fontSize 1.85, 방향 문구 fontSize 2.35 (3D TMP).
- 깃발 기둥 간 수평 간격 약 3.50m, 천 사이 약 2.54m.
- 새 Collider 총 8개: 안내면 BoxCollider 1, 안내판 기둥 2, 깃발 기둥 2, 짚단 원본 MeshCollider 3.
- 안내면 Collider: sign 로컬 center (0,1.60,-0.038), size (1.89,0.94,0.10). 보이는 목재면에 맞춤.
- 깃발 천/가로대/장식에는 Collider 없음. 추가 Invisible Wall 없음.
- Terrain Collider 및 XR 이동 레이어/설정 변경 없음.

## 검증 결과
- Unity Editor 기준 정적 배치 검증 완료.
- 저장 후 동일 원본 Scene 재열기 완료, isDirty=false.
- 시작 Main Camera 캡처에서 한글/화살표 표시 확인, 저장 후 누락 글리프 0.
- 제목 viewport (0.7075,0.4246,3.4667), 방향 문구 (0.7070,0.3252,3.4758): 카메라 앞 시야 안.
- Scene View에서 안내판·양쪽 깃발·짚단·기존 환경 상대 배치 확인.
- 첫 8m, 0.25m 간격 33지점에서 반지름 0.30m, 전체 높이 1.8m 캡슐 겹침 검사: 장애물 0. Terrain 및 XR 자신은 제외.
- XR 전체 컴포넌트 보존과 기하 충돌 검사로 확인. 실제 HMD 입력 이동/텔레포트/플레이 시간 측정은 하지 않음.
- 2~3초 인지 여부와 VR 실기 가독성/동선은 실제 사용자 HMD 검증 필요.
- 최종 Console Error 0, Warning 0. Console Clear하지 않음.
- 사방치기/장구 Scene 및 Build Settings SHA256 보존:
  - Assets/Scenes/JangGu.unity: 1CBEC348AD2E3FFDDB8FA4178706855D55BB9201D76E6A2A8F1A0BF73CD45496
  - Assets/Scenes/SaBang.unity: B326D2BC7DEC5F08E2AB67443BC8AEDC8F5D8F886CBB6C1DF325CE1DBFA00DE9
  - ProjectSettings/EditorBuildSettings.asset: 671105DC43D498C679E6E4132583C917AFA059E20142A4FAE9202B283B6516CA
- 검증 이미지 (프로젝트 Temp, Git 제외):
  - D:/work/ModooTraditionalPlay/Temp/EntranceInspection/entrance_verified_game.png
  - D:/work/ModooTraditionalPlay/Temp/EntranceInspection/entrance_final_scene-1.png

## 실제 변경 파일
- D:/work/ModooTraditionalPlay/Assets/Scenes/main_playoursound.unity
- D:/work/ModooTraditionalPlay/Assets/RouteGuide.meta
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance.meta
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Entrance_Indigo_Cloth.mat (+ .meta)
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Entrance_Ivory_Trim.mat (+ .meta)
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Entrance_Swallowtail_Placeholder.asset (+ .meta)
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Fonts.meta
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Fonts/Entrance_NotoSansKR_Bold_SDF.asset (+ .meta)
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Fonts/NotoSansKR-Bold.otf (+ .meta)
- D:/work/ModooTraditionalPlay/Assets/RouteGuide/Entrance/Fonts/OFL.txt (+ .meta)
- 작업 로그: D:/Codex/Log/20260904_전통마을입구길안내_Log.md (기존 파일이 있으면 추가 기록, 백업/삭제 없음).
- 로그는 저장소 외부 경로이므로 이 프로젝트의 Git 커밋에는 자동 포함되지 않음.

## 다음 작업 후보
1. 실제 XR HMD에서 시작 2~3초 인지, 문구 가독성, 정상 진행/충돌 확인.
2. 깃발 Placeholder의 전통 모델 교체.
3. 별도 승인 범위에서 입구 이후 공통 이동 경로와 Door01k (3) 장구 / Door01k (7) 사방치기 분기 안내.



---
# 2026-09-04 추가 작업: 잔치 마을 분위기

## 요청과 범위
- 사용자 요청: 길목을 잔치하는 집으로 유도하는 한국적 사물(깃발/천막 등)로 구성. 스토리 확정 후 추가 수정 예정.
- 현재 승인된 입구 첫 구간에 한정해 잔치 장식 적용. 기존 안내 문구/첫 진행 방향 유지.
- 대상: Assets/Scenes/main_playoursound.unity.
- 별도 백업 파일 생성 없음. 커밋/푸시 수행하지 않음.

## 적용 내용
- EntranceFlag_L/R에 오방색 계열 천 장식 각 5줄 추가. 기존 깃발 바탕을 소색으로 변경.
- 깃발 옆 청사초롱 형태 장식 각 1개, 천막 앞 양쪽 장식 2개: 총 4개.
- RouteGuide/Entrance/FeastCanopy: 소색 지붕, 남색 가장자리, 붉은 띠, 목재 기둥의 잔치 천막.
- 천막은 시작점 기준 진행 방향 7.6m, 오른쪽 4.4m 지점의 비통행 공간에 배치. 월드 (450.7855, 8.8494, 700.8588), Y 회전 148.6813도.
- 천막 지붕 약 폭 3.4m, 깊이 2.56m, 높이 2.2~2.88m. 외형에 맞는 목재 기둥 Collider 4개.
- 돗자리 1개, 소반 2개, 비빔밥 그릇 2개: 기존 환경 소품 재사용.
- 돗자리를 Terrain 표면 법선 (-0.0681, 0.9947, 0.0769)에 맞추고 소반/그릇 지면 높이 조정.
- 새 Collider는 기둥 4개와 기존 소반 프리팹 Collider 2개. 입구 전체 Collider 14개.
- 천/등/장식/그릇/돗자리는 추가 충돌 없음. 등은 시각 장식이며 Point Light 등 실제 조명 없음.
- 천막/등/천 장식은 스토리 및 최종 전통 에셋 확정 시 교체 가능한 Placeholder. 새 런타임 스크립트 없음.
- 천막 지붕은 기존 어두운 환경 아래에서도 표면이 읽히도록 전용 Unlit 재질 사용. Lighting 설정 변경 없음.

## 사용한 기존 에셋
- Assets/Naganeupseong/Prefabs/Prop/Small_Dining_Table.prefab
- Assets/Naganeupseong/Prefabs/Prop/Straw_Mat01a.prefab
- Assets/Naganeupseong/Prefabs/Prop/Bibimbap01a.prefab
- Assets/Naganeupseong/Resource/Materials/M_Kitchen_Board.mat
- 기존 입구 Entrance_Indigo_Cloth.mat / Entrance_Ivory_Trim.mat 재사용. 원본 KHS 파일 수정 없음.

## 이번 추가 파일
다음 경로 앞에는 D:/work/ModooTraditionalPlay/가 붙음. 각 파일의 Unity .meta 포함.
- Assets/RouteGuide/Entrance/Festival_Canopy_Cloth.mat
- Assets/RouteGuide/Entrance/Festival_Cloth_Canopy.asset
- Assets/RouteGuide/Entrance/Festival_Dark_Wood.mat
- Assets/RouteGuide/Entrance/Festival_Hanging_Ribbon.asset
- Assets/RouteGuide/Entrance/Festival_Red_Cloth.mat
- Assets/RouteGuide/Entrance/Festival_Unbleached_Cloth.mat
- 수정 Scene: Assets/Scenes/main_playoursound.unity
- 외부 로그: D:/Codex/Log/20260904_전통마을입구길안내_Log.md

## 주요 실제 Transform
Position/Rotation은 월드, Scale은 로컬.
| 오브젝트 | Position | Rotation | Scale |
| --- | --- | --- | --- |
| Entrance/EntranceFlag_L | (455.2988, 9.0437, 705.9460) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| EntranceFlag_L/Cheongsachorong_Placeholder | (455.6394, 11.0837, 706.1943) | (0.0000, 148.6813, 0.0000) | (0.9000, 0.9000, 0.9000) |
| Entrance/EntranceFlag_R | (452.3087, 8.8245, 704.1268) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| EntranceFlag_R/Cheongsachorong_Placeholder | (451.9318, 10.8645, 703.9384) | (0.0000, 148.6813, 0.0000) | (0.9000, 0.9000, 0.9000) |
| Entrance/FeastCanopy | (450.7855, 8.8494, 700.8588) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| FeastCanopy/CanopyLantern_L | (451.4610, 10.7294, 702.5574) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| FeastCanopy/CanopyLantern_R | (448.9665, 10.7294, 701.0396) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| FeastCanopy/FeastStrawMat | (450.7855, 8.8794, 700.8588) | (354.1974, 148.7344, 358.9527) | (1.6500, 1.0000, 1.0000) |
| FeastCanopy/Soban | (451.3152, 8.9194, 701.1810) | (0.0000, 148.6813, 0.0000) | (1.4000, 1.4000, 1.4000) |
| FeastCanopy/FeastBowl | (451.3152, 9.3064, 701.1810) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |
| FeastCanopy/Soban | (450.2558, 8.8999, 700.5365) | (0.0000, 148.6813, 0.0000) | (1.4000, 1.4000, 1.4000) |
| FeastCanopy/FeastBowl | (450.2558, 9.2869, 700.5365) | (0.0000, 148.6813, 0.0000) | (1.0000, 1.0000, 1.0000) |

## 보존 및 검증
- Unity 6000.3.10f1 / main_playoursound 단일 활성 Scene. 저장 후 원본 Scene 재열기 완료, 미저장 변경 없음.
- XR Rig Position (450.5939,9.55,709.6385), Rotation (1.30743086,148.6813,-0.000296923739), Scale (1,1,1): 이번 추가 작업 전후 동일.
- XR 전체 컴포넌트/AllLevel 환경/Directional Light 직렬화 해시 작업 전후 동일.
- 새 Light 0, 새 런타임 스크립트 0.
- Terrain 및 TerrainCollider 유지, Terrain Collider 물리재질 null 보존.
- 시작점 앞 8m, 0.25m 간격 33지점, 반지름 0.3m/높이 1.8m 캡슐 겹침 검사: 장애물 0.
- 시작 Main Camera에서 안내문과 깃발/등/천막 표시 확인. 근접 시점에서 돗자리/소반/그릇 및 지면 배치 확인.
- Scene View와 Game View 재확인. 최종 Console Error 0 / Warning 0.
- 장구/사방치기 Scene, Build Settings SHA256 이전 기록과 동일.
- 실제 HMD 이동, 2~3초 인지 시간, VR 가독성은 실기 검증 필요.
- 결과 이미지:
  - D:/work/ModooTraditionalPlay/Temp/EntranceInspection/festival_verified_game.png
  - D:/work/ModooTraditionalPlay/Temp/EntranceInspection/festival_verified_scene.png
  - D:/work/ModooTraditionalPlay/Temp/EntranceInspection/festival_canopy_detail.png

## 다음 작업
- 확정 스토리에 맞춰 초대 문구, 깃발/등 문양, 천막과 잔칫상의 소품 구성 구체화.
- HMD에서 안내 인지와 동선 실기 확인.
- 입구 이후 경로 확대 시 장구 Door01k (3), 사방치기 Door01k (7)까지 이어지는 장식/분기 안내를 별도 범위로 구성.




## 2026-09-04 15:26 KST — 사용자 Lit 수정 보존 및 인계 문서 정비

- 사용자가 천막 재질을 직접 Lit으로 수정한 뒤 추가 재질 전체 점검과 MD 관리를 요청했다.
- Unity MCP에서 Assets/RouteGuide/Entrance의 일반 .mat 6개와 실제 Renderer 연결을 확인: 모두 Universal Render Pipeline/Lit, Emission 비활성. 추가 일반 재질에 Unlit 잔존 없음.
- Festival_Canopy_Cloth.mat의 사용자 수정이 미저장 상태여서 해당 재질만 SaveAssetIfDirty로 저장했다. 이번 점검에서 셰이더를 새로 교체하지 않았다.
- TMP 글자는 TextMeshPro/Mobile/Distance Field 유지. 기존 환경 재질은 변경하지 않았다.
- 향후 Unlit 적용이나 조명 반응 변경은 사전에 사용자 확인을 받는다. 이전 로그의 천막 Unlit 기록은 현재 상태를 뜻하지 않는다.
- 현재 Lit 카메라 이미지: images/20260904-lit-current.png.
- 외부 로그를 저장소 docs/entrance-route-guide/WORKLOG.md로 이관하고 원본 보존. 최신 기준 HANDOFF.md 생성. 이후 이 두 문서로 현재 상태와 이력을 관리한다.
- Scene 및 XR Transform 변경 없음. Console Error 0 / Warning 0 확인. HMD 실기 검증은 수행하지 않았다.
- 이번 변경: Festival_Canopy_Cloth.mat 저장, HANDOFF.md, WORKLOG.md, 현재 상태 PNG. 별도 Scene 백업 및 Git 커밋·푸시는 수행하지 않았다.
- 다음 작업: HMD 실기 확인, 스토리 확정 후 한국적 장식과 초대 문구 정교화. 전체 경로 확대는 추가 요청 시 진행.
