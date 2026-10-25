(() => {
 const region=document.getElementById("appToastRegion");
 if(!region)return;
 const key="agentforge:toast";
 function show({title="",message="",type="info",duration=4000}={}){
  const kind=["success","error","warning","info"].includes(type)?type:"info";
  const item=document.createElement("div");
  item.className="app-toast app-toast--"+kind;
  item.setAttribute("role",kind==="error"?"alert":"status");
  const content=document.createElement("div");
  const heading=document.createElement("strong");
  heading.textContent=title;
  const description=document.createElement("span");
  description.textContent=message;
  content.append(heading,description);
  const close=document.createElement("button");
  close.type="button";
  close.textContent="×";
  close.setAttribute("aria-label","Dismiss notification");
  let timer;
  const dismiss=()=>{clearTimeout(timer);item.remove();};
  close.addEventListener("click",dismiss);
  item.append(content,close);
  region.append(item);
  timer=setTimeout(dismiss,duration);
 }
 function afterNavigation(notification){
  try{sessionStorage.setItem(key,JSON.stringify(notification));}
  catch{show(notification);}
 }
 window.AgentForgeToast=Object.freeze({show,afterNavigation});
 try{
  const pending=sessionStorage.getItem(key);
  if(pending){sessionStorage.removeItem(key);show(JSON.parse(pending));}
 }catch{}
})();
