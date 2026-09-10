"""Optional AI artwork regeneration. Existing PNG files need no API to use."""
import base64
import os
from pathlib import Path
import requests

PROMPTS = {
    'ships': '''Original professional 2D space shooter game sprite sheet, 1024x1024, pure black background, no text, no grid lines. Exactly four isolated spaceships arranged at centers of four equal quadrants with wide empty black margins. Top left: upward-facing sleek cyan and silver player spacecraft. Top right: downward-facing red and dark metallic small alien interceptor. Bottom left: downward-facing orange and bronze heavy alien gunship with broad armored wings. Bottom right: downward-facing violet alien command ship with curved symmetric wings. Each whole object occupies 60 percent of its quadrant. Top-down orthographic view, crisp illustrated metallic panels, bright colored emissive accents, strong readable silhouettes. No perspective, no stars, no shadows outside ships.''',
    'icons': '''Original 2D space shooter game sprite sheet, 1024x1024, pure black background, no text, no grid. Exactly four isolated luminous game icons centered in four equal quadrants with wide black margins. Top left: one narrow vertical cyan-white laser bolt, rounded tapered ends. Top right: green metallic hexagonal health pickup token with bright plus sign inside. Bottom left: gold metallic hexagonal rapid-fire pickup token with bright lightning bolt inside. Bottom right: blue metallic hexagonal shield pickup token with bright shield symbol inside. Each whole icon occupies 55 percent of its quadrant. Crisp polished sci-fi game UI art, no perspective. Four objects only.'''
}

def main():
    root = Path(__file__).resolve().parents[1]
    base = os.environ['IMAGE_API_BASE'].rstrip('/')
    for name, prompt in PROMPTS.items():
        path = root / 'ArtSource' / (name + '-original.png')
        if path.exists():
            continue
        response = requests.post(base + '/chat/completions', headers={
            'Authorization': 'Bearer ' + os.environ['IMAGE_API_KEY']}, json={
            'model': 'nano_banana_pro', 'modalities': ['image', 'text'],
            'image_config': {'aspect_ratio': '1:1', 'num_images': 1, 'rewrite_prompt': False},
            'messages': [{'role': 'user', 'content': [{'type': 'text', 'text': prompt}]}]}, timeout=240)
        if response.status_code != 200:
            raise RuntimeError('Image API response: ' + str(response.status_code) + ' ' + response.text[:1000])
        result = response.json()
        # Save the response locally for diagnosing provider-specific media formats.
        (root / 'ArtSource' / (name + '-response.json')).write_text(__import__('json').dumps(result))
        message = result['choices'][0]['message']
        import re
        images = message.get('images', [])
        if images:
            url = images[0].get('image_url', images[0].get('url'))
            if isinstance(url, dict): url = url['url']
        else:
            text = str(message.get('content', ''))
            urls = re.findall(r'https?://[^\s\)\]<>\"\']+', text)
            if not urls: raise RuntimeError('No image URL in provider response.')
            url = urls[0]
        if url.startswith('data:image'):
            content = base64.b64decode(url.split(',', 1)[1])
        else:
            download = requests.get(url, timeout=60)
            download.raise_for_status()
            content = download.content
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content)
        print('Saved generated artwork:', path.name, flush=True)

if __name__ == '__main__':
    main()
