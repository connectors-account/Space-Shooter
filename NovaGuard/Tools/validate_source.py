"""Linux-friendly source/config/asset checks. NOT a Unity compiler or playtest.
Install: python -m pip install -r Tools/requirements.txt
Run: python Tools/validate_source.py
"""
from pathlib import Path
import array
import json
import re
import unittest
import wave
import yaml
from PIL import Image
from tree_sitter import Language, Parser
import tree_sitter_c_sharp

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'Assets'
SCRIPTS = sorted(ASSETS.rglob('*.cs'))


def text(relative):
    return (ROOT / relative).read_text()


def unity_yaml(relative):
    source = text(relative)
    source = re.sub(r'^%.*\n', '', source, flags=re.M)
    source = re.sub(r'^--- !u!\d+ &\d+.*$', '---', source, flags=re.M)
    return yaml.safe_load(source)


class SourceChecks(unittest.TestCase):
    def test_all_csharp_syntax_trees(self):
        parser = Parser(Language(tree_sitter_c_sharp.language()))
        self.assertGreaterEqual(len(SCRIPTS), 15)
        for path in SCRIPTS:
            with self.subTest(path=path.name):
                tree = parser.parse(path.read_bytes())
                self.assertFalse(tree.root_node.has_error, str(tree.root_node))

    def test_component_filenames_match(self):
        for path in SCRIPTS:
            for name in re.findall(r'class\s+(\w+)\s*:\s*MonoBehaviour', path.read_text()):
                self.assertEqual(path.stem, name)

    def test_no_unimplemented_bodies(self):
        for path in SCRIPTS:
            self.assertNotRegex(path.read_text(), r'NotImplementedException|\bTODO\b|\bFIXME\b')

    def test_package_manifest_and_editor_version(self):
        packages = json.loads(text('Packages/manifest.json'))['dependencies']
        for module in ('ugui', 'modules.audio', 'modules.physics2d', 'modules.ui'):
            self.assertEqual(packages['com.unity.' + module], '1.0.0')
        self.assertIn('2022.3.62f3', text('ProjectSettings/ProjectVersion.txt'))

    def test_desktop_settings_and_input_backend(self):
        settings = unity_yaml('ProjectSettings/ProjectSettings.asset')['PlayerSettings']
        self.assertEqual(settings['activeInputHandler'], 0)
        self.assertEqual(settings['scriptingBackend']['Standalone'], 0)
        self.assertEqual(settings['defaultScreenWidth'], 1280)
        self.assertEqual(settings['defaultScreenHeight'], 720)
        setup = text('Assets/Editor/ProjectSetup.cs')
        self.assertIn('BuildTarget.StandaloneWindows64', setup)
        self.assertNotIn('BuildTarget.WebGL', setup)
        self.assertIn('NovaGuard.exe', setup)

    def test_ui_navigation_axes(self):
        axes = unity_yaml('ProjectSettings/InputManager.asset')['InputManager']['m_Axes']
        self.assertEqual({a['m_Name'] for a in axes}, {'Horizontal', 'Vertical', 'Submit', 'Cancel'})
        for axis in axes:
            self.assertTrue(axis['positiveButton'])
        keys = text('Assets/Scripts/KeyboardInput.cs')
        for key in ('W', 'A', 'S', 'D', 'UpArrow', 'DownArrow', 'LeftArrow', 'RightArrow', 'Space', 'Z', 'Escape', 'P', 'R', 'M'):
            self.assertRegex(keys, r'KeyCode\.' + key + r'\b')

    def test_layers_and_collision_contract(self):
        layers = unity_yaml('ProjectSettings/TagManager.asset')['TagManager']['layers']
        self.assertEqual(len(layers), 32)
        self.assertEqual(layers[8:13], ['Player', 'Enemy', 'PlayerBullet', 'EnemyBullet', 'Pickup'])
        physics = text('Assets/Scripts/PhysicsRules.cs')
        pairs = set(re.findall(r'IgnoreLayerCollision\((\w+), (\w+), false\)', physics))
        self.assertEqual(pairs, {('PlayerLayer', 'EnemyLayer'), ('PlayerLayer', 'HostileLayer'),
                                 ('PlayerLayer', 'PickupLayer'), ('EnemyLayer', 'FriendlyLayer')})

    def test_nine_sprite_import_sources(self):
        setup = text('Assets/Editor/ProjectSetup.cs')
        names = re.search(r'ArtNames = \{([^}]+)', setup).group(1)
        names = re.findall(r'"(\w+)"', names)
        self.assertEqual(len(names), 9)
        for name in names:
            with self.subTest(sprite=name):
                with Image.open(ASSETS / 'Art' / (name + '.png')) as sprite:
                    self.assertEqual(sprite.mode, 'RGBA')
                    self.assertLessEqual(max(sprite.size), 264)
                    low, high = sprite.getchannel('A').getextrema()
                    self.assertEqual(low, 0)
                    # Luminous AI-generated sprites can have near-opaque (253/254) cores.
                    self.assertGreaterEqual(high, 240)
                    self.assertGreater(sprite.width, 20)

    def test_audio_enum_matches_non_silent_pcm_files(self):
        sound = text('Assets/Scripts/SoundBank.cs')
        names = [x.strip() for x in re.search(r'enum SoundCue \{([^}]+)', sound).group(1).split(',')]
        self.assertEqual(len(names), 8)
        self.assertEqual({p.stem for p in (ASSETS / 'Audio').glob('*.wav')}, set(names))
        for name in names:
            with wave.open(str(ASSETS / 'Audio' / (name + '.wav'))) as clip:
                self.assertEqual((clip.getnchannels(), clip.getsampwidth(), clip.getframerate()), (1, 2, 44100))
                samples = array.array('h', clip.readframes(clip.getnframes()))
                self.assertGreater(max(samples), 500)
                self.assertLess(max(abs(v) for v in samples), 32767)

    def test_all_assets_have_unique_stable_guids(self):
        guids = []
        for path in ASSETS.rglob('*'):
            if path.suffix == '.meta':
                continue
            meta = Path(str(path) + '.meta')
            self.assertTrue(meta.exists(), str(path))
            content = yaml.safe_load(meta.read_text())
            self.assertRegex(content['guid'], r'^[0-9a-f]{32}$')
            guids.append(content['guid'])
        self.assertEqual(len(guids), len(set(guids)))

    def test_pause_restart_and_focus_source_contracts(self):
        game = text('Assets/Scripts/GameController.cs')
        self.assertIn('Time.timeScale = 0', game)
        self.assertIn('Time.timeScale = 1', game)
        self.assertIn('OnApplicationFocus', game)
        self.assertIn('OnApplicationPause', game)
        self.assertIn('child.gameObject.SetActive(false)', game)
        self.assertIn('waves.Stop()', game)
        self.assertIn('enemies.Clear()', game)
        self.assertIn('PlayerPrefs.Save()', game)
        self.assertIn('Application.Quit()', game)

    def test_menu_callbacks_resolve_to_methods(self):
        ui = text('Assets/Scripts/GameUI.cs')
        game = text('Assets/Scripts/GameController.cs')
        for method in re.findall(r', game\.(\w+)\);', ui):
            self.assertRegex(game, r'public void ' + method + r'\(')
        for phase in ('Menu', 'Playing', 'Paused', 'GameOver'):
            self.assertIn('GamePhase.' + phase, ui)

    def test_generation_safety_contracts(self):
        setup = text('Assets/Editor/ProjectSetup.cs')
        self.assertIn('EditorApplication.isCompiling || EditorApplication.isUpdating', setup)
        self.assertIn('SaveCurrentModifiedScenesIfUserWantsTo', setup)
        self.assertIn('PrefabUtility.SaveAsPrefabAsset', setup)
        self.assertIn('EditorSceneManager.SaveScene', setup)
        self.assertIn('ProjectValidation.Validate()', setup)
        self.assertIn('File.Exists(ScenePath)', setup)


if __name__ == '__main__':
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(SourceChecks)
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    report = {
        'scope': 'Static source, syntax trees, configuration and asset integrity only; not Unity compilation or runtime.',
        'checks_run': result.testsRun,
        'passed': result.wasSuccessful(),
        'csharp_files_parsed': len(SCRIPTS),
        'failures': [str(test) for test, _ in result.failures + result.errors],
        'unity_editor_tested': False,
        'windows_build_tested': False,
        'gameplay_tested': False,
    }
    (ROOT / 'docs' / 'SOURCE_CHECKS.json').write_text(json.dumps(report, indent=2) + '\n')
    raise SystemExit(0 if result.wasSuccessful() else 1)
