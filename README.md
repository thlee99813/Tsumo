# Tsumo

2D 탑뷰 전술 액션 게임. 제한 시간 안에 카드를 조합해 스쿼드를 구성하고, 시너지를 활용해 적을 처치하는 닌자 전투 게임

# Project Rules

## 1. 씬 관리 규칙
- 모든 씬 파일은 `00.Scenes` 폴더에서 관리합니다.
- 개인 작업 및 테스트는 개인용 Scene에서 진행합니다.
- 메인 씬은 사전 협의 없이 수정하지 않습니다.
- 씬 이름 예시: `TaeHwanScene_01`

## 2. 폴더 구조
- `00.Scenes`: 모든 씬 파일 (.unity)
- `01.Scripts`: 모든 C# 스크립트
- `02.Prefabs`: 재사용 가능한 프리팹 자산
- `03.Materials`: 머티리얼 및 관련 아트 자산
- `04.Art`: 3D 모델, 텍스처 등 아트 자산
- `05.UI`: UI 이미지, 스프라이트 및 UI 프리팹
- `06.Audio`: 사운드 이펙트(SFX) 및 배경음(BGM)
- `07.VFX`: 이펙트 및 파티클 시스템
- `08.Data`: ScriptableObject, JSON, CSV 등 데이터 파일
- `09.Fonts`: 폰트 파일 및 TMP 에셋
- `98.Debugger`: 디버깅 코드 및 테스트 파일
- `99.Test`: 개인 테스트용 자산 및 임시 파일 (`이름_Test` 형식 권장)
- `Editor`: 유니티 에디터 확장 및 커스텀 인스펙터 스크립트
- `Resources`: `Resources.Load`로 런타임 로드가 필요한 자산

## 3. Git 브랜치 전략
GitFlow를 기반으로 하며, 모든 작업은 피처 단위로 분리하여 진행합니다.

### 브랜치 구조
- `main`: 최종 배포 및 빌드용 브랜치
- `dev`: 개발 통합 브랜치
- `feature/*`: 단위 기능 구현 브랜치

### 피처 브랜치 명명 규칙
- 형식: `feature/기능명`
- 영문 소문자만 사용합니다.
- 공백은 하이픈(`-`)으로 대체합니다.
- 예시: `feature/player-movement`, `feature/enemy-ai`, `feature/ui-inventory`

## 4. 머지 및 충돌 관리 규칙
- `dev`에 머지하기 전, 원격 브랜치의 최신 내용을 확인합니다.
- `dev` 브랜치를 자신의 `feature` 브랜치로 먼저 머지하여 충돌 여부를 확인합니다.
- 충돌은 해당 기능 작업자가 직접 해결합니다.
- 컴파일 에러가 있는 상태에서는 `dev`로 머지하지 않습니다.
- 로컬 테스트 완료 후 문제가 없을 경우 `dev`로 머지합니다.

## 5. 코드 컨벤션
- `PascalCase`: 클래스, 메서드, 프로퍼티
- `_camelCase`: private 필드
- `camelCase`: 지역 변수, 파라미터
- 
## 6. 작업 흐름

1. `dev`에서 `feature/기능-이름` 브랜치를 생성합니다.
2. `00.Scenes` 내 개인용 Scene에서 기능을 구현하고 테스트합니다.
3. 구현 완료 후 `dev`의 최신 내용을 자신의 브랜치로 가져와 충돌 및 컴파일 상태를 확인합니다.
4. 문제가 없으면 `dev`로 머지합니다.
5. 최종 검증 완료 후 `main`에서 빌드를 진행하고 배포합니다.
```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private bool _isDead;

    public float MoveSpeed => _moveSpeed;

    public void Move(float moveInput)
    {
        float targetVelocity = moveInput * _moveSpeed;
    }
}
