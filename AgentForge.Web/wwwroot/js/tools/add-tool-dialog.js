(() => {
 const dialog=document.getElementById("addToolDialog");
 const open=document.getElementById("openAddToolDialog");
 if(!dialog||!open)return;
 const get=id=>document.getElementById(id);
 const status=get("addToolFeedback");
 const update=()=>{const kind=dialog.querySelector('input[name="toolKind"]:checked').value;dialog.querySelectorAll("[data-tool-panel]").forEach(p=>p.hidden=p.dataset.toolPanel!==kind);status.textContent="";};
 open.addEventListener("click",()=>dialog.showModal());
 dialog.querySelectorAll("[data-modal-close], [data-tool-close]").forEach(b=>b.addEventListener("click",()=>dialog.close()));
 dialog.querySelectorAll('input[name="toolKind"]').forEach(r=>r.addEventListener("change",update));
 get("externalToolSource").addEventListener("change",()=>{const integration=get("externalToolSource").value==="integration";get("externalEndpointField").hidden=integration;get("externalIntegrationField").hidden=!integration;});
 const rows=get("toolParameterRows");
 const schema=()=>{const properties={},required=[];rows.querySelectorAll(".ui-modal-parameter").forEach(r=>{const n=r.querySelector(".param-name").value.trim();if(!n)return;properties[n]={type:r.querySelector(".param-type").value};if(r.querySelector(".param-required").checked)required.push(n);});const value={type:"object",properties,required,additionalProperties:false};if(window.AgentForgeJsonViewer){window.AgentForgeJsonViewer.render(get("newToolSchema"),value);}else{get("newToolSchema").textContent=JSON.stringify(value,null,2);}};
 get("addToolParameter").addEventListener("click",()=>{const r=document.createElement("div");r.className="ui-modal-parameter";r.innerHTML='<input class="ui-input param-name" aria-label="Parameter name (required)" required pattern="[a-z][a-z0-9_]*" maxlength="155" placeholder="parameter_name" title="Required parameter name"><select class="ui-select param-type" aria-label="Parameter type"><option>string</option><option>integer</option><option>number</option><option>boolean</option><option>array</option><option>object</option></select><label><input type="checkbox" class="param-required" checked>Required</label><button type="button" class="ui-button param-remove" aria-label="Delete parameter" title="Delete parameter"><svg aria-hidden="true" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4h8v2M5 6l1 14h12l1-14M10 10v7M14 10v7"/></svg></button>';r.querySelector(".param-remove").onclick=()=>{r.remove();schema();};r.addEventListener("input",schema);r.addEventListener("change",schema);rows.append(r);schema();});
 const kindValues={custom:1,external:2};
 const categoryValues={general:1,development:2,integration:3,data:4,automation:5};
 const saveButton=get("previewToolConfiguration");
 let saving=false;
 const fieldError=(field,message)=>{
  field.setAttribute("aria-invalid","true");
  field.classList.add("border-red-600","ring-1","ring-red-600");
  let node=document.getElementById(field.id+"Error");
  if(!node){node=document.createElement("p");node.id=field.id+"Error";node.setAttribute("role","alert");field.insertAdjacentElement("afterend",node);}
  node.className="text-xs text-red-600";
  node.textContent=message;
  const described=field.getAttribute("aria-describedby")||"";
  if(!described.split(" ").includes(node.id))field.setAttribute("aria-describedby",(described+" "+node.id).trim());
 };
 const clearFieldError=field=>{
  field.removeAttribute("aria-invalid");
  field.classList.remove("border-red-600","ring-1","ring-red-600");
  const node=document.getElementById(field.id+"Error");
  if(node){node.textContent="";node.classList.add("hidden");}
 };
 dialog.addEventListener("input",e=>{if(e.target.matches("input,textarea,select"))clearFieldError(e.target);});
 dialog.addEventListener("cancel",e=>{if(saving)e.preventDefault();});
 dialog.querySelectorAll("[data-modal-close], [data-tool-close]").forEach(button=>button.addEventListener("click",e=>{if(saving){e.preventDefault();e.stopImmediatePropagation();}},true));
 saveButton.type="button";
 saveButton.addEventListener("click",async event=>{
  event.preventDefault();
  event.stopPropagation();
  if(saving)return;
  const kind=dialog.querySelector('input[name="toolKind"]:checked').value;
  if(kind!=="custom"){status.textContent="External tool discovery is not implemented yet.";return;}
  const name=get("newToolName").value.trim();
  const displayName=get("newToolDisplayName").value.trim();
  const description=get("newToolDescription").value.trim();
  const imageUrl=get("newToolImage").value.trim()||null;
  const category=categoryValues[get("newToolCategory").value];
  const errors=[];
  const requiredFields=[
   [get("newToolName"),"Tool name is required."],
   [get("newToolDisplayName"),"Display name is required."],
   [get("newToolDescription"),"Description is required."],
   [get("newToolCategory"),"Category is required."]
  ];
  for(const [field,message] of requiredFields){
   clearFieldError(field);
   if(!field.value.trim())errors.push([field,message]);
   else if(!field.checkValidity())errors.push([field,"Enter a valid value."]);
  }
  if(name&&!/^[a-z][a-z0-9_]*$/.test(name)&&!errors.some(([f])=>f===get("newToolName")))errors.push([get("newToolName"),"Use lowercase snake_case."]);
  if(!category&&!errors.some(([f])=>f===get("newToolCategory")))errors.push([get("newToolCategory"),"Select a valid category."]);
  if(imageUrl&&(!get("newToolImage").checkValidity()||!imageUrl.startsWith("https://")))errors.push([get("newToolImage"),"Enter a valid HTTPS URL."]);
  const parameterNames=new Set();
  for(const row of rows.querySelectorAll(".ui-modal-parameter")){
   const field=row.querySelector(".param-name");
   if(!field.id)field.id="toolParam"+Array.from(rows.children).indexOf(row);
   const value=field.value.trim();
   clearFieldError(field);
   if(!value)errors.push([field,"Parameter name is required."]);
   else if(!/^[a-z][a-z0-9_]*$/.test(value)||!field.checkValidity())errors.push([field,"Use lowercase snake_case."]);
   else if(parameterNames.has(value))errors.push([field,"Parameter names must be unique."]);
   parameterNames.add(value);
  }
  if(errors.length){
   errors.forEach(([field,message])=>fieldError(field,message));
   status.textContent="Correct the highlighted fields.";
   errors[0][0].focus();
   return;
  }
  const properties={},required=[];
  rows.querySelectorAll(".ui-modal-parameter").forEach(row=>{
   const key=row.querySelector(".param-name").value.trim();
   if(!key)return;
   properties[key]={type:row.querySelector(".param-type").value};
   if(row.querySelector(".param-required").checked)required.push(key);
  });
  const jsonSchema=JSON.stringify({type:"object",properties,required,additionalProperties:false});
  status.textContent="";
  saving=true;
  saveButton.disabled=true;
  saveButton.setAttribute("aria-busy","true");
  const originalContent=saveButton.innerHTML;
  saveButton.innerHTML='<span class="inline-flex items-center gap-2"><span class="inline-block size-4 animate-spin rounded-full border-2 border-current border-t-transparent" aria-hidden="true"></span>Saving…</span>';
  status.textContent="Saving tool...";
  try{
   const response=await fetch("/api/tools",{
    method:"POST",headers:{"Content-Type":"application/json"},
    body:JSON.stringify({name,displayName,description,kind:kindValues[kind],category,jsonSchema,imageUrl})
   });
   if(!response.ok){
    const contentType=response.headers.get("content-type")||"";
    const error=contentType.includes("json")?await response.json():await response.text();
    const message=typeof error==="string"?error:(error.detail||error.title||JSON.stringify(error.errors||error));
    throw new Error(message||"HTTP "+response.status);
   }
   status.textContent="Tool saved successfully.";
   dialog.close();
   window.location.reload();
  }catch(error){
   status.textContent="Could not save tool: "+(error.message||"Unknown error");
  }finally{saving=false;saveButton.disabled=false;saveButton.removeAttribute("aria-busy");saveButton.innerHTML=originalContent;}
 });
 update();schema();
 if(!window.AgentForgeJsonViewer){const script=document.createElement("script");script.src="/js/shared/json-viewer.js";script.onload=schema;document.head.append(script);}
})();