## Hướng Dẫn Setup LoadResource System

### Tổng Quan
System này sẽ tự động load file `quest.tsv` từ `StreamingAssets` khi game khởi động, update progress trên `UILoading`, sau đó chuyển sang Map1.

### Các Component Chính

#### 1. **LoadResource.cs** 
- **Chức năng**: Load quest.tsv từ StreamingAssets, parse TSV data
- **Location**: `Assets/_Game/Scripts/Services/API/LoadResource.cs`
- **Dependencies**: Cần QuestDataManager trong scene

#### 2. **QuestDataManager.cs**
- **Chức năng**: Singleton quản lý dữ liệu quest, cung cấp API để truy vấn
- **Location**: `Assets/_Game/Scripts/Manager/QuestDataManager.cs`
- **Cấu hình**: 
  - Có option `dontDestroyOnLoad` - dữ liệu sẽ persist qua các scene

#### 3. **ResourceLoader.cs**
- **Chức năng**: Orchestrate toàn bộ loading sequence
- **Location**: `Assets/_Game/Scripts/Services/API/ResourceLoader.cs`
- **Cấu hình**:
  - `loadResource`: Reference đến LoadResource component
  - `uiLoading`: Reference đến UILoading component
  - `nextSceneName`: Scene muốn load sau (default: "Map1")
  - `autoLoadNextScene`: Tự động load scene tiếp theo (default: true)
  - `minLoadingTime`: Thời gian tối thiểu loading screen (default: 2s)

### Setup Steps

#### Step 1: Tạo GameObject cho QuestDataManager

1. Trong scene (Menu hoặc Loading scene), tạo empty GameObject: `QuestDataManager`
2. Add component `QuestDataManager.cs`
3. Cấu hình:
   - `Don't Destroy On Load`: ON (để dữ liệu persist)

#### Step 2: Tạo GameObject cho LoadResource

1. Tạo empty GameObject: `ResourceLoader`
2. Add 2 components:
   - `LoadResource.cs`
   - `ResourceLoader.cs` (cái orchestrator)

#### Step 3: Gắn References trong ResourceLoader

1. Chọn `ResourceLoader` GameObject
2. Trong Inspector, gắn các references:
   - **Load Resource**: Kéo `ResourceLoader` GameObject vào (nó chứa LoadResource component)
   - **UI Loading**: Kéo UILoading Canvas vào
   - **Next Scene Name**: Nhập "Map1" (hoặc tên scene bạn muốn)
   - **Auto Load Next Scene**: Bật (ON)
   - **Min Loading Time**: 2 (hoặc tuỳ chỉnh)

#### Step 4: Xác nhận Scene Setup

- Tên các scene **PHẢI ĐÚNG** trong Build Settings:
  - Scene loading hiện tại (Menu/Loading)
  - "Map1" (hoặc tên scene chính)

### Cách Sử Dụng Quest Data

Trong các scripts khác, bạn có thể truy vấn dữ liệu như sau:

```csharp
// Lấy dữ liệu quest
var questData = QuestDataManager.Instance.GetQuestStep(questId: 1, stepId: 1);
Debug.Log(questData.titleQuest);
Debug.Log(questData.infoQuest);

// Lấy tất cả steps của quest 1
var steps = QuestDataManager.Instance.GetQuestSteps(questId: 1);

// Lấy quest info để hiển thị trên UI
string info = QuestDataManager.Instance.GetQuestInfo(1, 1);

// Lấy reward
var (coin, item, reward) = QuestDataManager.Instance.GetQuestReward(1, 1);
```

### Flow Execution

```
Game Start
  ↓
ResourceLoader.Start() 
  ↓
LoadingSequence() starts
  ↓
LoadResource.LoadAllResources()
  ↓
Đọc quest.tsv từ StreamingAssets
  ↓
Parse TSV data thành QuestData objects
  ↓
QuestDataManager.SetQuestData() - lưu dữ liệu
  ↓
UILoading.UpdateProgress() - hiển thị 0-100%
  ↓
Load xong (sau ≥ 2s)
  ↓
SceneManager.LoadScene("Map1")
```

### Troubleshooting

**Problem**: Quest data không load
- Check: File `quest.tsv` có trong folder `Assets/StreamingAssets/` không?
- Check: QuestDataManager có trong scene không?

**Problem**: Progress bar không update
- Check: UILoading Canvas có active không?
- Check: UILoading reference trong ResourceLoader inspector?

**Problem**: Scene không chuyển
- Check: Tên scene "Map1" đúng chưa?
- Check: Scene "Map1" đã add vào Build Settings chưa?
- Check: "Auto Load Next Scene" checkbox enabled?

### Mở Rộng

Nếu sau này cần load thêm resource khác (audio, sprites, ...):

1. Thêm method mới trong `LoadResource.cs` (ví dụ: `LoadAudioDataAsync()`)
2. Gọi method đó trong `LoadAllResourcesAsync()` coroutine
3. Update progress tương ứng
