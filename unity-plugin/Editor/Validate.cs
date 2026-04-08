using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 파라미터 검증 헬퍼. 실패 시 McpException을 던짐.
    /// Unity-Skills의 Validate 패턴을 MCP에 맞게 개선.
    /// </summary>
    public static class Validate
    {
        /// <summary>필수 문자열 파라미터 검증</summary>
        public static string Required(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new McpException(-32602, $"필수 파라미터 누락: {paramName}");
            return value;
        }

        /// <summary>필수 객체 파라미터 검증</summary>
        public static T Required<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new McpException(-32602, $"필수 파라미터 누락: {paramName}");
            return value;
        }

        /// <summary>JToken에서 필수 파라미터 추출 (참조/값 타입 모두 지원)</summary>
        public static T Required<T>(JToken p, string paramName)
        {
            var token = p?[paramName];
            if (token == null || token.Type == JTokenType.Null)
                throw new McpException(-32602, $"필수 파라미터 누락: {paramName}");
            if (token is T direct) return direct;
            return token.ToObject<T>();
        }

        /// <summary>숫자 범위 검증</summary>
        public static int InRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
                throw new McpException(-32602, $"{paramName}은(는) {min}~{max} 범위여야 합니다 (입력: {value})");
            return value;
        }

        /// <summary>float 범위 검증</summary>
        public static float InRange(float value, float min, float max, string paramName)
        {
            if (value < min || value > max)
                throw new McpException(-32602, $"{paramName}은(는) {min}~{max} 범위여야 합니다 (입력: {value})");
            return value;
        }

        /// <summary>
        /// 안전한 에셋 경로 검증.
        /// Assets/ 또는 Packages/로 시작해야 하며, .. 경로 탐색 차단.
        /// </summary>
        /// <summary>단일 파라미터 SafeAssetPath (paramName 기본값: "path")</summary>
        public static string SafeAssetPath(string path) => SafeAssetPath(path, "path");

        public static string SafeAssetPath(string path, string paramName)
        {
            Required(path, paramName);

            if (path.Contains(".."))
                throw new McpException(-32602, $"경로에 '..'를 사용할 수 없습니다: {path}");

            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("Assets\\", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("Packages\\", StringComparison.OrdinalIgnoreCase))
            {
                throw new McpException(-32602, $"경로는 Assets/ 또는 Packages/로 시작해야 합니다: {path}");
            }

            return path.Replace('\\', '/');
        }

        /// <summary>
        /// 안전한 파일 시스템 경로 검증 (프로젝트 루트 내).
        /// </summary>
        public static string SafeFilePath(string assetPath, string paramName)
        {
            Required(assetPath, paramName);
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                throw new McpException(-32602, $"프로젝트 루트 외부 경로 접근 거부: {assetPath}");

            return fullPath;
        }

        /// <summary>Enum 파싱 검증</summary>
        public static T ParseEnum<T>(string value, string paramName) where T : struct, Enum
        {
            if (!Enum.TryParse<T>(value, true, out var result))
                throw new McpException(-32602, $"유효하지 않은 {paramName}: '{value}'. 가능한 값: {string.Join(", ", Enum.GetNames(typeof(T)))}");
            return result;
        }

        /// <summary>양수 검증</summary>
        public static int Positive(int value, string paramName)
        {
            if (value <= 0)
                throw new McpException(-32602, $"{paramName}은(는) 양수여야 합니다 (입력: {value})");
            return value;
        }
    }
}
