# EasyHCI (OnePack fork)

[한국어](#한국어) · [English](#english)

메모리 오버클럭 안정화 검증에 쓰는 **HCI MemTest**를 한글화하고, 여러 개를 자동으로 띄워 굴리기 편하게 만든 런처입니다.
[Manbocoon/EasyHCI](https://github.com/Manbocoon/EasyHCI)를 원작자(MIT, © 2022 kbum08) 표기를 유지한 채 이어받아 관리하는 포크입니다.

---

## 한국어

### 이 포크가 원본과 다른 점

| 항목 | 원본 | 이 포크 |
|---|---|---|
| 클린 클론 빌드 | 실패 (`app.config`, `FodyWeavers.xml` 누락) | 성공 |
| `memtest.exe` | 동봉 + 실행 파일에 내장 후 추출 | **미동봉**. 이미 가지고 있는 사본을 찾아 씁니다 |
| Fody / Costura | `packages/` 30개 파일 동봉, IL 위빙 | 제거. 위빙 없는 일반 빌드 |
| `.gitignore`, CI, 릴리스 | 없음 | 있음 |
| 코어·메모리 배분 | 레지스트리로 쓰레드 수를 세고, 탐색 상한이 3530MB로 고정된 채 아래로만 내려감 | 런타임 쓰레드 수를 쓰고, 쓰레드당 몫에서 탐색을 시작해 램을 끝까지 씀 |
| MemTest 창 제어 | 창과 버튼을 **한글 캡션으로** 찾음 (`어서오세요!`, `테스트 시작`, `무료 버전`) | **컨트롤 ID로** 찾음. 한글판·영문판 모두 동작 |
| 통과율·오류 읽기 | 한국어 문구의 **글자 위치를 잘라내서** 파싱. 영문 빌드에서는 매번 예외 | 숫자를 읽어서 파싱. 문구와 무관 |
| 버전 | 1.0.8D 고정 | 1.3.1 |

`memtest.exe`를 뺀 이유는 라이선스입니다. HCI MemTest는 HCI Design의 독점 프리웨어이고,
다른 프로그램 안에 넣어 배포하려면 허가가 필요합니다. 원본은 실행 파일 안에 넣고 첫 실행 때 꺼내 쓰는 방식이라,
이 포크는 그 방식을 쓰지 않습니다. 자세한 내용은 [THIRD_PARTY_NOTICES](THIRD_PARTY_NOTICES.md)에 있습니다.

### 설치

1. [hcidesign.com/memtest](https://hcidesign.com/memtest/)에서 HCI MemTest를 받습니다. (무료 버전으로 충분합니다)
2. 받은 `memtest.exe`를 `EasyHCI.exe` 옆에 둡니다.

```
EasyHCI.exe
EasyHCI.exe.config
MaterialSkin.dll
memtest.exe          <- 여기에
```

다른 곳에 두고 쓰고 싶으면 `Resources\memtest_path.txt` 파일에 전체 경로 한 줄을 적으면 됩니다.
이 파일이 있으면 아래 자동 탐색보다 항상 우선합니다.

### memtest.exe를 찾는 순서

1. `Resources\memtest_path.txt`에 적힌 경로
2. `Resources\memtest.exe` (예전 버전이 쓰던 위치)
3. `EasyHCI.exe`와 같은 폴더
4. 위로 최대 6단계 올라가며 `hci-memtest\memtest.exe`
5. 같은 단계에서 `tools` 폴더를 찾으면 그 안의 `hci`, `memtest`가 들어간 폴더만 골라 검색

5번 덕분에 원팩 같은 포터블 묶음에 넣으면 별도 설정 없이 바로 찾습니다.
못 찾으면 찾아본 경로 전체를 보여주는 안내 창이 뜹니다.

### 사용법

1. **설정** 탭에서 목표치를 정합니다. 필수는 아니고, 그대로 둬도 됩니다.
2. **테스트** 탭에서 테스트 시작을 누릅니다.
3. 진행 상황은 테스트 탭에서 볼 수 있습니다. 오래 걸리므로 잘 때나 외출할 때 돌리는 걸 권합니다.

- 테스트 중 메모리 사용량이 95% 이상 유지되어야 결과를 믿을 수 있습니다.
- 최대 할당량·CPU 쓰레드 수·여유 메모리는 자동으로 잡으므로 따로 계산하지 않아도 됩니다.
- 자동 캡처, 소리 알림, 로그 기록, 메모리 정리, 종료 후 자동 전원 끄기까지 원본 기능은 그대로 있습니다.

### 주의

- HCI MemTest가 절대적인 기준은 아닙니다. TM5, Prime95 같은 도구와 함께 쓰는 편이 좋습니다.
- 장시간 통과해도 실사용에서 문제가 생길 수 있습니다. 대개는 램 온도 문제이고, 게임 중에는 GPU 열기까지 더해집니다.
  높은 전압으로 오버클럭했다면 램 스팟쿨링을 권합니다.
- 이 프로그램은 관리자 권한으로 실행됩니다. HCI MemTest가 메모리를 크게 잡으려면 필요합니다.

### 직접 빌드

Visual Studio(또는 MSBuild)와 .NET Framework 4.6.2 타기팅 팩이 필요합니다.

```powershell
msbuild EasyHCI.sln /t:Rebuild /p:Configuration=Release
```

결과물은 `bin\Release`에 `EasyHCI.exe`, `EasyHCI.exe.config`, `MaterialSkin.dll` 세 개입니다.

### 라이선스

MIT. 원본 저작권 표기는 [LICENSE](LICENSE)에 그대로 남아 있습니다.

---

## English

A Korean-localized launcher for **HCI MemTest** that automates the tedious part of memory
overclock stability testing: picking a per-instance allocation, spawning the right number of
instances, monitoring coverage, and shutting the machine down when the run finishes.

This is a maintained fork of [Manbocoon/EasyHCI](https://github.com/Manbocoon/EasyHCI), keeping the
original MIT notice (© 2022 kbum08).

### What this fork changes

| | Upstream | This fork |
|---|---|---|
| Clean-clone build | Fails (`app.config` and `FodyWeavers.xml` are missing) | Succeeds |
| `memtest.exe` | Bundled and embedded into the executable, then extracted | **Not bundled.** Uses the copy you already have |
| Fody / Costura | 30 vendored files plus IL weaving | Removed, no weaving |
| `.gitignore`, CI, releases | None | Present |
| Version | Pinned at 1.0.8D | 1.3.1 |
| Core and memory planning | Registry walk for the thread count; probe capped at 3530 MB and only ever lowered | Runtime thread count; probe starts from the per-thread share so the whole memory can be used |
| MemTest window control | Windows and buttons found by **Korean captions** (`어서오세요!`, `테스트 시작`, `무료 버전`) | Found by **control id**, so both the Korean and English builds work |
| Coverage and error readout | Parsed by **chopping Korean text at fixed offsets**, which threw on every English status line | Parses the numbers, so the wording does not matter |

`memtest.exe` was removed for licensing reasons. HCI MemTest is proprietary freeware owned by
HCI Design, and shipping it inside another program needs the author's permission. Upstream embeds
the binary and extracts it at first run; this fork does not. Details are in
[THIRD_PARTY_NOTICES](THIRD_PARTY_NOTICES.md).

### Install

1. Download HCI MemTest from [hcidesign.com/memtest](https://hcidesign.com/memtest/). The free edition is enough.
2. Put `memtest.exe` next to `EasyHCI.exe`.

To keep it elsewhere, write the full path on one line into `Resources\memtest_path.txt`; that file
always wins over the automatic search.

### Search order for memtest.exe

1. The path in `Resources\memtest_path.txt`
2. `Resources\memtest.exe` (where older releases expected it)
3. The folder holding `EasyHCI.exe`
4. Up to six parent levels, looking for `hci-memtest\memtest.exe`
5. At each level, a `tools` folder is searched only inside subfolders whose names contain `hci` or `memtest`

Step 5 is what lets it work inside a portable pack with no configuration. When nothing is found, the
dialog lists every location that was searched.

### Usage

1. Set your target on the **설정** (Settings) tab. Optional.
2. Press start on the **테스트** (Test) tab.
3. Watch progress there. Runs are long, so start them overnight.

Memory usage should stay above 95 percent for a result worth trusting. Maximum allocation, CPU thread
count and free memory are detected automatically. The original features are unchanged: automatic
screenshots, sound alerts, logging, memory cleanup, and automatic shutdown when a run completes.

### Warnings

- HCI MemTest is not the only authority on stability. Pair it with TM5, Prime95 and real workloads.
- A long passing run can still fail in daily use, usually because memory runs hotter in games once GPU
  heat rises. Consider spot cooling if you run high voltage.
- The app requests administrator rights, which HCI MemTest needs for large allocations.

### Build

```powershell
msbuild EasyHCI.sln /t:Rebuild /p:Configuration=Release
```

Requires MSBuild and the .NET Framework 4.6.2 targeting pack. Output is `EasyHCI.exe`,
`EasyHCI.exe.config` and `MaterialSkin.dll` in `bin\Release`.

### License

MIT. The original copyright notice is preserved in [LICENSE](LICENSE).
