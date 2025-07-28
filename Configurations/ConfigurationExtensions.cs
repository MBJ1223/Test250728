using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// Entity Configuration 확장 메서드
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// JsonDocument 속성 변환 설정
        /// </summary>
        public static PropertyBuilder<JsonDocument?> ConfigureJson(
            this PropertyBuilder<JsonDocument?> propertyBuilder,
            string columnType = "JSON",
            string? comment = null)
        {
            propertyBuilder
                .HasColumnType(columnType)
                .HasConversion(
                    new ValueConverter<JsonDocument?, string?>(
                        v => v == null ? null : v.RootElement.GetRawText(),
                        v => v == null ? null : JsonDocument.Parse(v, new JsonDocumentOptions())
                    ),
                    new ValueComparer<JsonDocument?>(
                        (l, r) => JsonDocumentComparer.Compare(l, r),
                        v => v == null ? 0 : JsonDocumentComparer.GetHashCode(v),
                        v => v == null ? null : JsonDocument.Parse(v.RootElement.GetRawText(), new JsonDocumentOptions())
                    )
                );

            if (!string.IsNullOrEmpty(comment))
            {
                propertyBuilder.HasComment(comment);
            }

            return propertyBuilder;
        }

        /// <summary>
        /// Enum을 문자열로 저장하는 설정
        /// </summary>
        public static PropertyBuilder<TEnum> ConfigureEnumAsString<TEnum>(
            this PropertyBuilder<TEnum> propertyBuilder,
            int maxLength = 50,
            string? comment = null)
            where TEnum : struct, Enum
        {
            propertyBuilder
                .HasMaxLength(maxLength)
                .HasConversion<string>();

            if (!string.IsNullOrEmpty(comment))
            {
                propertyBuilder.HasComment(comment);
            }

            return propertyBuilder;
        }

        /// <summary>
        /// 필수 문자열 속성 설정
        /// </summary>
        public static PropertyBuilder<string> IsRequiredString(
            this PropertyBuilder<string> propertyBuilder,
            int maxLength,
            string? comment = null)
        {
            propertyBuilder
                .IsRequired()
                .HasMaxLength(maxLength);

            if (!string.IsNullOrEmpty(comment))
            {
                propertyBuilder.HasComment(comment);
            }

            return propertyBuilder;
        }

        /// <summary>
        /// 선택적 문자열 속성 설정
        /// </summary>
        public static PropertyBuilder<string?> IsOptionalString(
            this PropertyBuilder<string?> propertyBuilder,
            int maxLength,
            string? comment = null)
        {
            propertyBuilder
                .HasMaxLength(maxLength);

            if (!string.IsNullOrEmpty(comment))
            {
                propertyBuilder.HasComment(comment);
            }

            return propertyBuilder;
        }

        /// <summary>
        /// Decimal 속성 설정
        /// </summary>
        public static PropertyBuilder<decimal> ConfigureDecimal(
            this PropertyBuilder<decimal> propertyBuilder,
            int precision,
            int scale,
            string? comment = null)
        {
            propertyBuilder
                .HasPrecision(precision, scale);

            if (!string.IsNullOrEmpty(comment))
            {
                propertyBuilder.HasComment(comment);
            }

            return propertyBuilder;
        }
    }

    /// <summary>
    /// JsonDocument 비교를 위한 헬퍼 클래스
    /// </summary>
    internal static class JsonDocumentComparer
    {
        public static bool Compare(JsonDocument? left, JsonDocument? right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            return left.RootElement.GetRawText() == right.RootElement.GetRawText();
        }

        public static int GetHashCode(JsonDocument document)
        {
            return document.RootElement.GetRawText().GetHashCode();
        }
    }
}