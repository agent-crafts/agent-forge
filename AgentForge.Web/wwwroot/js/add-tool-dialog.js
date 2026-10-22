(() => {
 const dialog=document.getElementById("addToolDialog");
 const open=document.getElementById("openAddToolDialog");
 if(!dialog||!open)return;
 const get=id=>document.getElementById(id);
 const status=get("addToolFeedback");
 const update=()=>{const kind="custom";dialog.querySelectorAll("[data-tool-panel]").forEach(p=>p.hidden=p.dataset.toolPanel!==kind);status.textContent="";};
 open.addEventListener("click",()=>dialog.showModal());
 dialog.querySelectorAll("[data-tool-close]").forEach(b=>b.addEventListener("click",()=>dialog.close()));
 dialog.querySelectorAll('input[name="toolKind"]').forEach(r=>r.addEventListener("change",update));
 get("externalToolSource").addEventListener("change",()=>{const integration=get("externalToolSource").value==="integration";get("externalEndpointField").hidden=integration;get("externalIntegrationField").hidden=!integration;});
 const rows=get("toolParameterRows");
 const schema=()=>{const properties={},required=[];rows.querySelectorAll(".ui-modal-parameter").forEach(r=>{const n=r.querySelector(".param-name").value.trim();if(!n)return;properties[n]={type:r.querySelector(".param-type").value};if(r.querySelector(".param-required").checked)required.push(n);});get("newToolSchema").textContent=JSON.stringify({type:"object",properties,required,additionalProperties:false},null,2);};
 get("addToolParameter").addEventListener("click",()=>{const r=document.createElement("div");r.className="ui-modal-parameter";r.innerHTML='<input class="ui-input param-name" aria-label="Parameter name" placeholder="parameter_name"><select class="ui-select param-type" aria-label="Parameter type"><option>string</option><option>integer</option><option>number</option><option>boolean</option><option>array</option><option>object</option></select><label><input type="checkbox" class="param-required">Required</label><button type="button" class="ui-button param-remove" aria-label="Delete parameter" title="Delete parameter"><svg aria-hidden="true" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4h8v2M5 6l1 14h12l1-14M10 10v7M14 10v7"/></svg></button>';r.querySelector(".param-remove").onclick=()=>{r.remove();schema();};r.addEventListener("input",schema);r.addEventListener("change",schema);rows.append(r);schema();});
 get("previewToolConfiguration").addEventListener("click",()=>{const custom=true;if(custom){const name=get("newToolName").value.trim();if(!/^[a-z][a-z0-9_]*$/.test(name)||!get("newToolDisplayName").value.trim()||!get("newToolDescription").value.trim()){status.textContent="Enter a valid tool name, display name and description.";return;}schema();status.textContent="Preview ready. Registry persistence is not implemented yet.";}else{status.textContent="Source selected. Secure discovery and import are not implemented yet.";}});update();schema();
})();