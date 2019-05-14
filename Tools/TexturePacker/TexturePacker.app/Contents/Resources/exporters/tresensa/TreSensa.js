// Created with TexturePacker http://www.codeandweb.com/texturepacker
// {{smartUpdateKey}}
TGE.AssetManager.SpriteSheets["{{texture.trimmedName}}"] = {
{% for sprite in allSprites %} 
	"{{sprite.trimmedName}}":[{{sprite.frameRect.x}}, {{sprite.frameRect.y}}, {{sprite.frameRect.width}}, {{sprite.frameRect.height}}]{% if not forloop.last %}, {% endif %}{% endfor %}
};
