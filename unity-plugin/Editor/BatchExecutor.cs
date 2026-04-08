using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 제네릭 배치 처리 프레임워크.
    /// per-item 에러 격리, setup/teardown 훅, 집계 결과 리포트.
    /// Unity-Skills의 BatchExecutor 패턴을 개선 (크기 제한 추가).
    /// </summary>
    public static class BatchExecutor
    {
        public const int MaxBatchSize = 500;

        /// <summary>
        /// JArray에서 아이템을 역직렬화하여 배치 처리.
        /// </summary>
        /// <typeparam name="TItem">각 아이템의 타입</typeparam>
        /// <param name="items">JSON 배열</param>
        /// <param name="processor">각 아이템을 처리하는 함수</param>
        /// <param name="setup">배치 시작 전 호출 (optional, e.g. AssetDatabase.StartAssetEditing)</param>
        /// <param name="teardown">배치 완료 후 호출 (optional, e.g. AssetDatabase.StopAssetEditing)</param>
        public static object Execute<TItem>(
            JArray items,
            Func<TItem, int, object> processor,
            Action setup = null,
            Action teardown = null)
        {
            if (items == null || items.Count == 0)
                throw new McpException(-32602, "items 배열이 비어있습니다.");

            if (items.Count > MaxBatchSize)
                throw new McpException(-32602, $"배치 크기 초과: {items.Count}개 (최대 {MaxBatchSize}개)");

            var results = new List<object>();
            int successCount = 0;
            int failCount = 0;

            setup?.Invoke();
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    try
                    {
                        var item = items[i].ToObject<TItem>();
                        var result = processor(item, i);
                        results.Add(new { index = i, success = true, result });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { index = i, success = false, error = ex.Message });
                        failCount++;
                    }
                }
            }
            finally
            {
                teardown?.Invoke();
            }

            return new
            {
                success = failCount == 0,
                totalItems = items.Count,
                successCount,
                failCount,
                results,
            };
        }

        /// <summary>
        /// 간단한 JToken 배열 배치 처리 (역직렬화 없이).
        /// </summary>
        public static object Execute(
            JArray items,
            Func<JToken, int, object> processor,
            Action setup = null,
            Action teardown = null)
        {
            if (items == null || items.Count == 0)
                throw new McpException(-32602, "items 배열이 비어있습니다.");

            if (items.Count > MaxBatchSize)
                throw new McpException(-32602, $"배치 크기 초과: {items.Count}개 (최대 {MaxBatchSize}개)");

            var results = new List<object>();
            int successCount = 0;
            int failCount = 0;

            setup?.Invoke();
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    try
                    {
                        var result = processor(items[i], i);
                        results.Add(new { index = i, success = true, result });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { index = i, success = false, error = ex.Message });
                        failCount++;
                    }
                }
            }
            finally
            {
                teardown?.Invoke();
            }

            return new
            {
                success = failCount == 0,
                totalItems = items.Count,
                successCount,
                failCount,
                results,
            };
        }

        /// <summary>
        /// 에셋 편집 배치 (StartAssetEditing/StopAssetEditing 자동 호출).
        /// </summary>
        public static object ExecuteAssetBatch(JArray items, Func<JToken, int, object> processor)
        {
            return Execute(items, processor,
                setup: () => AssetDatabase.StartAssetEditing(),
                teardown: () => AssetDatabase.StopAssetEditing());
        }

        /// <summary>인덱스 불필요 시 사용하는 간편 오버로드</summary>
        public static object Execute(JArray items, Func<JToken, object> processor, Action setup = null, Action teardown = null)
            => Execute(items, (item, _) => processor(item), setup, teardown);

        /// <summary>인덱스 불필요 시 사용하는 에셋 배치 간편 오버로드</summary>
        public static object ExecuteAssetBatch(JArray items, Func<JToken, object> processor)
            => ExecuteAssetBatch(items, (item, _) => processor(item));
    }
}
