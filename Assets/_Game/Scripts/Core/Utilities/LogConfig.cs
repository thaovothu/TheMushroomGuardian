// using UnityEngine;

// /// <summary>
// /// Quản lý Debug.Log để tránh tụt FPS:
// /// - Bản build (Player): luôn TẮT hẳn log.
// /// - Trong Unity Editor: mặc định TẮT log cho mượt (giống build). Khi cần debug, đổi
// ///   <see cref="EnableLogsInEditor"/> = true rồi để Unity compile lại.
// ///
// /// Cách bật/tắt nhanh khi đang chơi (không cần đổi code/compile):
// ///   gõ vào ô gì đó hoặc gọi: Debug.unityLogger.logEnabled = true;  // bật lại tạm thời
// ///
// /// Lưu ý: khi log bị tắt, chuỗi trong Debug.Log($"...") VẪN được dựng (cấp phát) trước
// /// khi gọi. Với log nằm trong vòng lặp mỗi frame thì nên xóa hẳn để khỏi tạo rác.
// /// </summary>
// public static class LogConfig
// {
//     // ── CÔNG TẮC ────────────────────────────────────────────────────────────────
//     // Đổi thành true khi cần xem Debug.Log trong Editor để debug.
//     // Để false khi muốn Editor chạy mượt (mặc định).
//     public const bool EnableLogsInEditor = true;
//     // ────────────────────────────────────────────────────────────────────────────

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void Init()
//     {
// #if UNITY_EDITOR
//         if (EnableLogsInEditor)
//         {
//             // Vẫn giữ log để debug, nhưng bỏ capture stack trace cho Log & Warning
//             // (phần tốn nhất). Error & Exception giữ stack trace đầy đủ để truy vết.
//             Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
//             Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
//         }
//         else
//         {
//             // Tắt log trong Editor cho mượt gần như build.
//             Debug.unityLogger.logEnabled = false;
//         }
// #else
//         // Bản build: luôn tắt hẳn log.
//         Debug.unityLogger.logEnabled = false;
// #endif
//     }
// }
