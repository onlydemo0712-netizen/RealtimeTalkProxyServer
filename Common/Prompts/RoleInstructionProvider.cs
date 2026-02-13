using System.Text.Json;

namespace ElderAIServer.Common.Prompts
{
    public enum InstructionScene
    {
        Base,
        Welcome
    }

    public static class RoleInstructionProvider
    {
        public static string GetInstructions(PromptTemplate tpl, InstructionScene scene)
        {
            if (tpl == null) throw new ArgumentNullException(nameof(tpl));

            // template.instructions.base / template.instructions.welcome
            if (!tpl.Template.TryGetProperty("instructions", out var instObj) ||
                instObj.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Prompt template missing: template.instructions");
            }

            string key = scene switch
            {
                InstructionScene.Welcome => "welcome",
                _ => "base"
            };

            // welcome 不存在就 fallback base
            if (key == "welcome" &&
                (!instObj.TryGetProperty("welcome", out var welcomeEl) || welcomeEl.ValueKind != JsonValueKind.String))
            {
                key = "base";
            }

            if (!instObj.TryGetProperty(key, out var el) || el.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException($"Prompt template missing: template.instructions.{key}");
            }

            return el.GetString() ?? string.Empty;
        }

        // 小幫手：順便取 roleId（可選）
        public static string GetRoleId(PromptTemplate tpl)
        {
            if (tpl.Template.TryGetProperty("roleId", out var roleIdEl) && roleIdEl.ValueKind == JsonValueKind.String)
                return roleIdEl.GetString() ?? string.Empty;

            return string.Empty;
        }
    }
}
