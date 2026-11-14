"""Run pure input/timing contracts with Unity's bundled .NET compiler; no editor license needed."""
import json
import os
from pathlib import Path
import subprocess
import tempfile

root = Path(__file__).resolve().parents[1]
editor = Path(os.environ.get('UNITY_EDITOR_ROOT', '/opt/unity/Editor'))
dotnet = editor / 'Data/NetCoreRuntime/dotnet'
compiler = editor / 'Data/DotNetSdkRoslyn/csc.dll'
runtimes = list((editor / 'Data/NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
runtime = max(runtimes, key=lambda p: tuple(int(n) for n in p.name.split('.')))
with tempfile.TemporaryDirectory(prefix='fantasy-input-contracts-') as directory:
    work = Path(directory)
    output = work / 'InputContracts.dll'
    sources = [root / 'Tests/GameplayInputContracts.cs',
               root / 'Assets/Scripts/Runtime/CombatPointerState.cs',
               root / 'Assets/Scripts/Runtime/CombatActionClock.cs',
               root / 'Assets/Scripts/Runtime/GameplayViewport.cs']
    if os.environ.get('CAMPAIGN_CONTRACTS'):
        sources = [root / 'Tests/CampaignContracts.cs'] + [root / ('Assets/Scripts/Runtime/' + name + '.cs') for name in ['CampaignCatalog', 'CampaignQuestLog', 'GameSave']]
    if os.environ.get('CAMPAIGN_FLOW_CONTRACTS'):
        sources = [root / 'Tests/CampaignFlowContracts.cs'] + [root / ('Assets/Scripts/Runtime/' + name + '.cs') for name in ['CampaignCatalog', 'CampaignQuestLog', 'GameSave', 'CampaignInventory', 'ChapterOne']]
    args = ['/nologo', '/target:exe', '/nostdlib+', f'/out:"{output}"']
    args += [f'/reference:"{p}"' for p in runtime.glob('*.dll')]
    args += [f'"{p}"' for p in sources]
    response = work / 'compile.rsp'
    response.write_text('\n'.join(args))
    subprocess.run([str(dotnet), str(compiler), '@' + str(response)], check=True)
    (work / 'InputContracts.runtimeconfig.json').write_text(json.dumps({
        'runtimeOptions': {'tfm': 'net6.0', 'framework': {
            'name': 'Microsoft.NETCore.App', 'version': runtime.name}}}))
    subprocess.run([str(dotnet), str(output)], check=True)
