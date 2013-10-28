Shader "Custom/Simple Transparent" {
    Properties {
    	_Color ("Color & Transparency", Color) = (1,1,1,1)
    }
    SubShader {
	    LOD 200
	    Lighting Off
	    ZWrite Off
	    Cull Back
	    Blend SrcAlpha OneMinusSrcAlpha
	    Tags {"Queue" = "Transparent"}
	    Color[_Color]
	    Pass {
	    }
    } 
    FallBack "Diffuse"
}