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

## 2026-09-11 KST — Naganeupseong 기존 재질 재사용 분석

- Unity MCP로 `Assets/Naganeupseong`의 Material 335개와 `Unreal/PBR_Shaders` 연결 구조를 조사했다.
- 천·목재·돗자리 후보의 Base Color/Normal/Roughness/AO 텍스처와 현재 제작 Mesh UV를 비교했다.
- 가장 유력한 재사용 후보는 연속적인 목재 결을 가진 `Assets/Naganeupseong/Resource/Materials/M_Pillar01b.mat`. 천막 기둥·깃대·가로대에 시험 가능하나 실제 화면 비교 후 적용한다.
- `M_Cloth_Roller`, `M_Bed_Clothes`, `M_Pillow`, `M_Mat`은 원본 소품용 UV 아틀라스라 현재 천막·깃발에 직접 사용하기 부적합하다.
- `M_Straw_Mat01a`는 돗자리에는 적합하지만 잔칫천이나 깃발에는 부적합하다.
- `Festival_Hanging_Ribbon.asset`은 UV가 없어 텍스처 기반 재질을 직접 사용할 수 없다.
- Scene, 기존 재질, XR, Terrain을 변경하지 않았다. 별도 백업 및 Git 커밋·푸시 없음.

## 2026-09-11 — 깃발 재질 비교본 생성

사용자 요청으로 원본 왼쪽 깃발 옆에 동일 형태의 비교본을 생성했다.
- 경로: RouteGuide/Entrance/EntranceFlag_L_MaterialComparison
- Position: (456.83650, 9.14307, 706.88170). 원본에서 바깥쪽 1.8m, Terrain 높이에 맞춤. 회전·스케일·메시 형태는 원본과 동일.
- 목재 기둥/가로대/등 프레임: Assets/Naganeupseong/Resource/Materials/M_Pillar01b.mat
- 넓은 깃발 천: Assets/Naganeupseong/Resource/Materials/M_Bed_Clothes.mat
- 리본과 등 종이 등 나머지는 원본 재질 유지. 재질 에셋 자체 수정 없음.
- 비교본 Collider는 비활성화. 비교용 임시 배치이며 최종 동선용 배치가 아님.
- 원본 Transform 및 XR Transform 동일 확인, TerrainCollider 활성 유지, Console Error/Warning 0. 대상 Scene 저장 완료.
- 이미지: images/20260911-flag-material-comparison.png. 화면 왼쪽 붉은 천이 비교본, 가운데 밝은 천이 원본 왼쪽 깃발.
- 변경 파일: Assets/Scenes/main_playoursound.unity, 이 문서와 WORKLOG.md, 위 PNG. 백업 파일·커밋·푸시 없음. HMD 미검증.
- 다음 작업은 사용자 비교 결과에 따라 적용 재질을 결정하고 비교용 배치를 정리하는 것.

## 2026-09-11 — 사용자 제공 천 재질 5종 비교 제작

사용자 제공 D:/2026 윤우상/마테리얼의 ZIP 5개에서 텍스처만 추출. 외부 다운로드 없음. 원본 압축파일 보존.
- 생성: RouteGuide/Entrance/FabricMaterialComparisons 아래 동일 형태 깃발 5개. 공통 목재 M_Pillar01b, 넓은 천만 각 소재로 변경. 리본/등 종이 유지. Collider 모두 비활성.
- Sample_1 crepe_satin_2k: (457.2307,9.1910,709.6967)
- Sample_2 denim_fabric_06_2k: (458.7684,9.1925,710.6324)
- Sample_3 jeans-fabric-unity: (460.3061,9.2227,711.5680)
- Sample_4 quatrefoil_jacquard_fabric_2k: (461.8438,9.2541,712.5037)
- Sample_5 twill-fabric-unity: (463.3815,9.2682,713.4393)
- 新 재질은 모두 Unreal/PBR_Shaders. Albedo sRGB, 데이터맵 Linear, OpenGL Normal은 NormalMap importer 사용. 2K 제한, mipmap 활성.
- jeans/twill metallic PSD는 Unity 패킹 관례인 R=Metallic, Alpha=Smoothness로 해석해 별도 Metallic 및 1-Alpha Roughness 맵 생성. 제공 명세는 없어 이 채널 의미는 추정이며 재질 최종 승인 전 확인 대상.
- jacquard AO는 ARM의 R에서 추출. metallic 맵 없는 denim은 비금속 0. Displacement/Height는 사용하지 않음.
- 비교용 천 Mesh는 원본 복제 후 Tangent 재계산, 형상/UV 유지. 원본 Mesh 수정 없음.
- 파일: Assets/RouteGuide/Entrance/FabricSamples/ (텍스처24개, 파생맵, 재질5개, 비교메시5개 및 meta), Assets/Scenes/main_playoursound.unity, 이 MD/WORKLOG와 images/20260911-five-fabric-samples.png, 20260911-fabric-comparison-close.png, 20260911-jacquard-detail.png.
- 근접 비교 이미지의 새 샘플 5개는 왼쪽부터 twill, jacquard, jeans, denim, crepe satin 순서. 오른쪽 뒤에는 이전 비교본과 원본 깃발이 보임.
- XR Transform 보존 확인, Console Error/Warning 0, Scene 저장. HMD 미검증. 비교용 임시 배치로 최종 배치 아님. 별도 백업/커밋/푸시 없음.
- 다음: 사용자가 천을 선택하면 최종 깃발에 반영하고 비교군 정리. 현재 원본 2개는 그대로 보존.

## 2026-09-11 — 잔치마을 에셋 구성 방향 확정

사용자가 지정한 군기/영기/KHS Flag A·B·C/천막/Linen/자카드·새틴의 역할을 FESTIVAL_ART_DIRECTION.md에 정리했다. 다음 제작 시 해당 명세를 우선 참고한다. 개별 모델 링크 및 실제 도입 여부는 아직 확정되지 않았으며 이번에는 MD만 작성했다.
사용자가 비교용 깃발을 삭제했다고 알렸고 직전 Unity MCP 확인에서 FabricMaterialComparisons와 EntranceFlag_L_MaterialComparison은 없었으며 EntranceFlag_L/R는 남아 있었다. 당시 Scene은 미저장 상태였다. 이전 생성 로그와 이미지는 과거 기록이며 현재 배치를 뜻하지 않는다. 삭제한 비교본을 다시 생성하지 않는다.

## 2026-09-11 10:43 — 실제 유산 깃발 입구 배치 완료

최신 사용자 결정: KHS Flag A/B 제외. 사용자 제공 팔달위 ZIP, 장안위 ZIP, Flag C GLB만 사용.
- RouteGuide/Entrance/HeritageRouteFlags 생성. Southern_Entrance (455.29880,9.04372,705.94600), Northern_Entrance (452.30870,8.82447,704.12680), FlagC_Next_1 (456.96210,9.18276,703.21230), FlagC_Next_2 (453.97210,9.01274,701.39310).
- 회전 Y 148.6813도. 군기 원본 약4.77m → 0.65배, Flag C 천 0.7배 및 높이2.35m. Flag C는 천만 있어 기존 M_Pillar01b 재질 기둥 추가.
- 기존 EntranceFlag_L/R는 삭제 없이 비활성화. 사용자가 삭제한 비교본은 복구하지 않음. 사용자 미저장 삭제 상태를 포함하여 대상 Scene 저장.
- GLB를 정적 메시·텍스처로 변환. 신규 패키지 없음. 노드 변환/좌표계/삼각형 winding/UV 변환, 앞뒤 표시용 메시 면 복제, Tangent 재계산. 애니메이션 없음.
- Assets/RouteGuide/Entrance/HeritageFlags 아래 Southern/Northern/FlagC 각각 메시 및 Unreal/PBR_Shaders 재질 생성. Albedo sRGB, NormalMap, Roughness/Metallic/AO linear, 2K 제한. 원본 GLB 문양 유지. 원본 압축파일 수정 없음.
- 변경 Scene은 main_playoursound만. XR Transform 전후 동일, TerrainCollider 활성, 전방8m 33지점 캡슐검사 장애물0. Console Error/Warning 0. HMD 실기 미검증.
- 이미지: images/20260911-heritage-flags-start.png. 이전 비교 이미지는 현재 상태가 아님.
- 변경 파일: Assets/Scenes/main_playoursound.unity, Assets/RouteGuide/Entrance/HeritageFlags/ 및 meta, MD/로그/위 PNG. Temp/convert_route_flags.py는 변환 작업용 임시 도구이며 런타임 스크립트 아님.
- 별도 백업/커밋/푸시 없음. 다음 후보: HMD에서 깃발 가림과 방향 인지 확인, 사용자 의견에 따라 높이·반복 간격 조정. 전체 게임장까지 경로 확장은 이번에 하지 않음.

## 2026-09-11 — EntranceFlag로 두 게임장까지 길 안내 확장

사용자가 전체 길목 안내와 차분한 EntranceFlag 사용을 요청하여 기존 입구 한정 범위를 두 목적지 문 앞까지 확장했다.
- HeritageRouteFlags 비활성화. EntranceFlag_L/R 재활성화. 기존 재질 보존, Unlit 변경 없음.
- RouteGuide/EntranceFlagRoutes: 공통구간7, 사방치기방향5(문 앞 포함), 장구문앞1이 아니라 총12개: CommonRoute_0..6 7개, SabangchigiRoute_7..10 4개, Janggu_DoorApproach_11 1개. 0.85배 복제, 약6m 간격, Collider 비활성.
- Terrain 높이 및 Physics 캡슐 장애물 지도를 기반으로 동선 산출. 장구는 문 남쪽 (466,667), 사방치기는 접근 가능한 문 앞 (488.5,664)까지 연결. 닫힌 문 통과는 보장하지 않음.
- 갈림길에 접근 시 화면 기준 왼쪽 사방치기/오른쪽 장구 표지. 각 문 앞에 목적지명 표지. NotoSansKR 기존 원본에서 RouteDestination_SDF.asset 추가, 필요한 글리프 포함.
- 실제 Transform은 route-flag-transforms.json 참고. 이미지 images/20260911-entranceflag-junction.png.
- XR Transform 동일, TerrainCollider 활성, Console Error/Warning0. main_playoursound 저장. 실제 HMD 전체 이동은 미검증.
- 변경: 대상 Scene, Fonts/RouteDestination_SDF.asset 및 meta, 문서/Transform JSON/이미지. 새 런타임 스크립트·패키지·백업·커밋·푸시 없음.
- 다음: HMD 전체 동선 따라가며 깃발 가림과 간격 및 분기 표지 가독성 조정.

## 2026-09-11 — 사용자 수정 보존, 깃발 L/R 정비 및 문 트리거 전환

- Unity MCP 확인: Unity 6000.3.10f1 / XRI 3.3.1, 활성 Assets/Scenes/main_playoursound.unity. 시작 시 사용자 미저장 변경 있음. 사용자 배치/삭제 상태를 그대로 이어서 저장.
- RouteGuide/EntranceFlagRoutes 현재 9개 유지. 삭제된 SabangchigiRoute_9/_10 및 Janggu_DoorApproach_11 복구하지 않음.
- SabangchigiRoute_7, SabangchigiRoute_8: 담장 방향 Raycast와 화면 확인 후 기존 EntranceFlag_R의 자식 localPosition.x 사용. 천/리본/가로대와 등불/등걸이 좌우 교체. 각 자식의 y/z, 회전, 크기 및 재질 유지. 깃대 루트 Transform 변경 없음.
- _7 Position (474.56980,9.58876,665.25760), Rotation (0,106.01210,0), Scale (0.85,0.85,0.85).
- _8 Position (481.29350,9.58473,664.28280), Rotation (0,110.55600,0), Scale (0.85,0.85,0.85).
- 전체 현재 Transform: route-flag-transforms.json. CommonRoute_3..6 등의 사용자 직접 이동 반영.
- AllLevel/Door01k (3) → Assets/Scenes/JangGu.unity.
- AllLevel/Door01k (7) → Assets/Scenes/SaBang.unity.
- 각 문에 XRSimpleInteractable + DoorSceneTransition 추가. 기존 MeshCollider를 상호작용 대상으로 등록; Collider 형상/활성/물리 설정 변경 없음. 비볼록 MeshCollider의 ClosestPoint 문제를 피하도록 interactable 거리 계산은 TransformPosition 사용.
- 활성 Left Controller/Right Controller의 기존 NearFarInteractor를 명시 연결. Activate 바인딩은 각각 <XRController>{LeftHand}/{TriggerButton}, <XRController>{RightHand}/{TriggerButton}.
- 문을 hover 중인 같은 컨트롤러의 새 트리거 입력만 LateUpdate에서 판정. 그립/select 불필요. 기존 XR 입력/이동 설정 변경 없음. Scene 비동기 Single 로드 및 중복 전환 방지.
- ProjectSettings/EditorBuildSettings.asset에 두 목적지 Scene 활성 등록. 기존 SampleScene 및 순서 보존. 목적지 Scene을 열거나 저장하지 않음. 빌드 첫 Scene은 기존 SampleScene 그대로.
- XR 전후 동일(Transform 직렬화 비교): Position (450.5939,9.55,709.6385), Euler (1.30743086,148.6813,-0.000296923739), Scale (1,1,1).
- Terrain / TerrainCollider 활성 유지. 추가 깃발 Collider 비활성 유지. 새 재질/Unlit 변경 없음.
- Editor 검증: 스크립트 컴파일 성공, Console Error/Warning 전후 0. 임시 오브젝트를 이용한 입력 판정 8개 검사 통과: 비hover, 좌/우 fresh trigger, held trigger, 반대 손, 미등록 interactor, 비활성 controller, null. 임시 테스트 오브젝트는 finally에서 제거.
- Scene View 및 Main Camera Game View 확인. 실제 컨트롤러 ray→hover와 목적지 로드는 HMD 실기 검증 필요. 목적지 Scene 보존을 위해 실제 Scene 전환 테스트는 실행하지 않음.
- 변경: Assets/Scenes/main_playoursound.unity; Assets/RouteGuide/Entrance/Scripts.meta 및 Scripts/DoorSceneTransition.cs(.meta); ProjectSettings/EditorBuildSettings.asset; docs/entrance-route-guide/HANDOFF.md, WORKLOG.md, route-flag-transforms.json, door-trigger-validation.cs.txt 및 images/20260911-door-route-flags-LR.png, 20260911-door-flags-scene-view.png, 20260911-door-interaction-game-view.png.
- 백업 Scene/파일, 커밋, 푸시 없음. Git diff의 변경 Scene은 main_playoursound 하나.
- 다음: HMD에서 양손 조준→트리거로 각 게임 진입, 누른 채 시선 이동 시 오작동 여부 확인. 배포 첫 Scene 변경은 별도 요청 시 처리.

## 2026-09-11 — 스크립트 저장 위치 통일
- 사용자 지침: 앞으로 작성하는 프로젝트 스크립트는 Assets/Script 폴더에 저장한다.
- DoorSceneTransition.cs를 Assets/RouteGuide/Entrance/Scripts에서 Assets/Script/DoorSceneTransition.cs로 Unity AssetDatabase.MoveAsset을 사용하여 이동했다. meta GUID와 Scene 컴포넌트 참조 유지. 기능 변경 없음.
- 과거 기록의 이전 스크립트 경로는 작업 당시 경로이며 현재 경로는 Assets/Script/DoorSceneTransition.cs이다.
