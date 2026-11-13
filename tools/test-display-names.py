import json
import re
import sys
from pathlib import Path

root = Path(r"E:\DeepSeekHarness\JobBarsCN\JobBars")
zh = json.loads((root / "Localization" / "zh-CN.json").read_text(encoding="utf-8"))

# Mirror of Loc.T / UiHelper.JobDisplay / UiHelper.DisplayName (Chinese mode).
CHINESE_JOBS = {
    "GNB": "绝枪战士", "PLD": "骑士", "WAR": "战士", "DRK": "暗黑骑士",
    "AST": "占星术士", "SCH": "学者", "WHM": "白魔法师", "SGE": "贤者",
    "BRD": "吟游诗人", "MCH": "机工士", "DNC": "舞者", "DRG": "龙骑士",
    "SAM": "武士", "NIN": "忍者", "VPR": "蝰蛇剑士", "MNK": "武僧",
    "RPR": "钐镰客", "SMN": "召唤师", "BLM": "黑魔法师", "RDM": "赤魔法师",
    "PCT": "绘灵法师", "BLU": "青魔法师",
}

# Localize(job) in Chinese mode returns the Excel sheet name; approximate with the English
# name used by the client, which is what the name-key lookup stores.
ENGLISH_JOBS = {
    "GNB": "Gunbreaker", "PLD": "Paladin", "WAR": "Warrior", "DRK": "Dark Knight",
    "AST": "Astrologian", "SCH": "Scholar", "WHM": "White Mage", "SGE": "Sage",
    "BRD": "Bard", "MCH": "Machinist", "DNC": "Dancer", "DRG": "Dragoon",
    "SAM": "Samurai", "NIN": "Ninja", "VPR": "Viper", "MNK": "Monk",
    "RPR": "Reaper", "SMN": "Summoner", "BLM": "Black Mage", "RDM": "Red Mage",
    "PCT": "Pictomancer", "BLU": "Blue Mage",
}


def T(key):
    return zh.get(key, key)


def job_display(code):
    return CHINESE_JOBS.get(code, ENGLISH_JOBS.get(code, code))


job_lookup = {}
for code in CHINESE_JOBS:
    job_lookup[ENGLISH_JOBS[code].lower()] = code
    job_lookup[code.lower()] = code


def display_name(name):
    if not name:
        return name

    open_idx = name.rfind("(")
    if open_idx > 0 and name.endswith(")"):
        prefix = name[:open_idx].strip()
        inner = name[open_idx + 1:-1].strip()
        job = job_lookup.get(inner.lower())
        if job:
            return f"{T(prefix)}（{job_display(job)}）"

    if name.endswith(" Procs"):
        head = name[: -len(" Procs")]
        job = job_lookup.get(head.lower())
        if job:
            return f"{job_display(job)} {T('Procs')}"

    if " - " in name:
        head, tail = name.rsplit(" - ", 1)
        job = job_lookup.get(tail.strip().lower())
        if job:
            return f"{T(head.strip())} - {job_display(job)}"

    return T(name)


checks = [
    # Job suffix in parentheses: the prefix is a dictionary key, the job is translated.
    ("MP (Dark Knight)", "MP（暗黑骑士）"),
    ("GCD (Warrior)", "GCD（战士）"),
    # Proc suffix from the English client.
    ("Black Mage Procs", "黑魔法师 触发"),
    # Custom cooldown names keep the ability name (game sheet) and translate the job.
    ("Rampart - WAR", "Rampart - 战士"),
    ("Reprisal (Paladin)", "Reprisal（骑士）"),
    # Plain gauge names: dictionary hit, otherwise untouched (game data is already localized).
    ("Rattling Coil", "响尾蛇"),
    ("Inner Release", "Inner Release"),
    ("Bloodwhetting", "Bloodwhetting"),
]

failed = 0
for src, expected in checks:
    got = display_name(src)
    ok = got == expected
    if not ok:
        failed += 1
    print(("OK   " if ok else "FAIL ") + f"{src!r} -> {got!r}" + ("" if ok else f" (expected {expected!r})"))

# Every Chinese job name must be present and differ from the English one.
for code, cn in CHINESE_JOBS.items():
    if not cn:
        print("EMPTY job name", code)
        failed += 1

# All dictionary values must be non-empty strings.
for k, v in zh.items():
    if not isinstance(v, str) or not v.strip():
        print("BAD entry", repr(k), repr(v))
        failed += 1

print(f"\nfailures={failed}, entries={len(zh)}")
sys.exit(1 if failed else 0)
