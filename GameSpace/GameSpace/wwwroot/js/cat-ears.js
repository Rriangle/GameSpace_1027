// GameSpace Cat Ears JS (unchanged from v1)
(function(){
  const maxTilt = 12;
  function bind(el){
    const tilt = el.dataset.tilt === "1";
    if(tilt){
      el.addEventListener('mousemove', (e)=>{
        const r = el.getBoundingClientRect();
        const x = (e.clientX - r.left) / r.width;
        const left = (0.5 - x) * maxTilt;
        const right = (x - 0.5) * maxTilt;
        el.style.setProperty('--ear-left',  left.toFixed(2) + 'deg');
        el.style.setProperty('--ear-right', right.toFixed(2) + 'deg');
      });
      el.addEventListener('mouseleave', ()=>{
        el.style.removeProperty('--ear-left');
        el.style.removeProperty('--ear-right');
      });
    }
    el.addEventListener('click', ()=>{
      el.classList.add('ear-wiggle');
      setTimeout(()=> el.classList.remove('ear-wiggle'), 420);
    });
  }
  document.addEventListener('DOMContentLoaded', ()=>{
    document.querySelectorAll('.cat-ear-wrapper').forEach(bind);
  });
})();
