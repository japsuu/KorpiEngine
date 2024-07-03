frame start

for each camera:
- create/update size of g-buffer
- set RenderingCamera to this
- update graphics matrices
- clear RenderingCamera
- prepare the rendering pipeline
- for each object:
  - invoke OnPreRender
  - if directional light:
    - for all objects:
      - call OnRenderObjectDepth
        - render depth info to shadow map
- for each opaque (non-light) object:
  - render object to g-buffer
- 