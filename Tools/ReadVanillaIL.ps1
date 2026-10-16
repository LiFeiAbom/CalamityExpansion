param(
	[string]$Type,
	[string]$Assembly = 'D:\Game\Steam\steamapps\common\tModLoader\tModLoader.dll',
	[switch]$ListMethods,
	[switch]$ListFields,
	[string]$FindField,
	[string]$Method,
	[string]$Needle,
	[int]$Before = 40,
	[int]$After = 200
)

# 原版行为实查脚本（2026-10-16 新增）：用 Mono.Cecil 读 tModLoader.dll（内含合并进来的原版类），
# dump 某个方法的 IL、或列出某类型的成员，用来确认原版钩子名/数值/粒子写法。
# 用法：
#   -Type Terraria.NPC -ListMethods
#   -Type Terraria.ID.ItemID -ListFields
#   -Type Terraria.NPC -Method UpdateNPC_BuffApplyVFX -Needle daybreak -Before 8 -After 260
# ASCII only on purpose: PowerShell 5.1 reads this file as ANSI, and mangled CJK bytes eat the following newline.
# ASCII only on purpose: PowerShell 5.1 reads this file as ANSI, and mangled CJK bytes eat the following newline.
$cecil = 'D:\Game\Steam\steamapps\common\tModLoader\Libraries\mono.cecil\0.11.6\lib\netstandard2.0\Mono.Cecil.dll'
$dll = $Assembly
Add-Type -Path $cecil
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($dll)
if ($FindField) {
	# search every type in the module for a field whose name matches, then dump its initial-value bytes
	foreach ($tt in $asm.MainModule.Types) {
		foreach ($f in $tt.Fields) {
			if ($f.Name -like $FindField) {
				$hex = ($f.InitialValue | ForEach-Object { $_.ToString() }) -join ','
				Write-Output ("{0}::{1} = [{2}]" -f $tt.FullName, $f.Name, $hex)
			}
		}
	}
	exit 0
}
$t = $asm.MainModule.GetType($Type)
if ($null -eq $t) { Write-Output "type not found: $Type"; exit 1 }
if ($ListMethods) {
	foreach ($m in $t.Methods) { Write-Output $m.FullName }
	exit 0
}
if ($ListFields) {
	foreach ($f in $t.Fields) {
		if ($f.HasConstant) { Write-Output ("{0} = {1}" -f $f.Name, $f.Constant) }
		elseif ($f.HasInitialValue) { Write-Output ("{0} = [{1}]" -f $f.Name, (($f.InitialValue | ForEach-Object { $_.ToString() }) -join ',')) }
	}
	exit 0
}
foreach ($m in $t.Methods) {
	if (-not $m.HasBody) { continue }
	if ($Method -and $m.Name -ne $Method) { continue }
	$ins = $m.Body.Instructions
	for ($i = 0; $i -lt $ins.Count; $i++) {
		$op = $ins[$i].Operand
		if ($null -eq $op) { continue }
		if ($op.ToString() -match $Needle) {
			Write-Output ("=== {0}::{1}  IL_{2:x4}  matched: {3}" -f $Type, $m.Name, $ins[$i].Offset, $op.ToString())
			$s = [Math]::Max(0, $i - $Before)
			$e = [Math]::Min($ins.Count - 1, $i + $After)
			for ($k = $s; $k -le $e; $k++) {
				Write-Output ("IL_{0:x4}: {1,-14} {2}" -f $ins[$k].Offset, $ins[$k].OpCode, $ins[$k].Operand)
			}
			Write-Output ''
		}
	}
}
