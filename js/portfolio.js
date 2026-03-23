(function($){
  $(function(){
    /* scripts.min.js가 모든 header a에 preventDefault + $(href) 스크롤을 거는데,
       href가 projects/... 또는 dawnstar.html 등이면 선택자가 무효라 네비가 죽음 — proj-header만 일반 링크 동작 */
    $('header.proj-header a').off('click');

    // Theme toggle
    var theme = localStorage.getItem('theme') || 'light';
    $('body').attr('data-theme', theme);
    $('#theme-toggle').on('click', function(){
      var t = $('body').attr('data-theme') === 'dark' ? 'light' : 'dark';
      $('body').attr('data-theme', t);
      localStorage.setItem('theme', t);
    });

    // i18n
    var dict = {
      ko: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'포트폴리오 페이지','common.process':'개발 프로세스 페이지',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'저는 단순히 스크립트를 작성하는 것뿐만 아니라 생태계 구축에 대한 깊은 열정을 가진 게임 개발자이자 시스템 아키텍트, 월드 디자이너입니다. 픽셀과 코드에 생명을 불어넣어 시뮬레이션 중심의 세계와 역동적인 AI를 만드는 것이 진정한 원동력입니다.',
        'about.p2':'저는 PCG 던전 제너레이터에서 끝없는 절차적 미로를 생성하고, 2D MMORPG에서 원활한 네트워크 상호작용을 만들어냈으며, 멀티플레이어 협동 프로젝트에서 협업 게임플레이에 생명을 불어넣었습니다. 전통적인 게임 논리를 넘어 딥러닝 분류 모델을 개발하여 데이터와 AI의 교차점을 탐구하기도 했습니다.',
        'about.p3':'제 작업은 Unreal Engine, Unity, 확장 가능한 백엔드 서버, 머신 러닝을 포함한 다양한 환경과 기술에 걸쳐 있으며, 다양한 시스템에서 복잡한 기술적 매듭을 풀 수 있는 직접적인 경험을 제공합니다.',
        'about.p4':'저의 프로 여정은 이제 막 시작되었지만, 제 야망은 무궁무진합니다. 저는 끊임없이 배우고, 기술적 경계를 허물며, 게임 제작의 다음 큰 도전을 간절히 받아들이고자 하는 열망에 힘입어 성장하고 있습니다.',
        'about.cta':'이력서 다운로드'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'Portfolio page','common.process':'Development process',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'I am not someone who merely writes scripts—I am a game developer, systems architect, and world designer with a deep passion for building ecosystems. Breathing life into pixels and code to create simulation-driven worlds and dynamic AI is what truly drives me.',
        'about.p2':'I have generated endless procedural mazes in a PCG dungeon generator, built seamless network interactions in a 2D MMORPG, and brought cooperative gameplay to life in a multiplayer co-op project. Beyond traditional game logic, I have also developed deep learning classification models to explore the intersection of data and AI.',
        'about.p3':'My work spans Unreal Engine, Unity, scalable backend servers, machine learning, and many other environments and technologies—offering hands-on experience untangling complex technical knots across diverse systems.',
        'about.p4':'My professional journey has only just begun, but my ambition is boundless. I am growing fueled by the drive to keep learning, to push technical boundaries, and to eagerly embrace the next great challenge in game development.',
        'about.cta':'Download Resume'
      }
    };
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        // attr: 점(.)이 들어간 키(common.process)는 jQuery .data()로 조회 시 깨질 수 있음
        var key = $(this).attr('data-i18n');
        if(dict[lang] && key && dict[lang][key]){
          $(this).text(dict[lang][key]);
        }
      });
      $('#lang-toggle').text(lang==='en'?'EN':'KR');
    }
    var lang = localStorage.getItem('lang') || 'ko';
    applyI18n(lang);
    $('#lang-toggle').on('click', function(){
      lang = (lang==='ko'?'en':'ko');
      localStorage.setItem('lang', lang);
      applyI18n(lang);
    });

    // IntersectionObserver for reveal and lazy video
    var io = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if(entry.isIntersecting){
          $(entry.target).addClass('in-view');
          var video = $(entry.target).find('video.work-video')[0];
          if(video && !video.src && video.dataset.src){
            video.src = video.dataset.src;
          }
        }
      });
    }, {threshold: 0.2});
    $('.reveal').each(function(){ io.observe(this); });

    // Lazy hero video iframe injection
    var heroDiv = document.querySelector('.hero-video');
    if(heroDiv && heroDiv.dataset.videoSrc){
      var observer = new IntersectionObserver(function(es){
        es.forEach(function(e){
          if(e.isIntersecting){
            var iframe = document.createElement('iframe');
            iframe.setAttribute('src', heroDiv.dataset.videoSrc);
            iframe.setAttribute('frameborder','0');
            iframe.setAttribute('allow','autoplay; encrypted-media');
            heroDiv.appendChild(iframe);
            observer.disconnect();
          }
        });
      }, {threshold:0.1});
      observer.observe(heroDiv);
    }

    // Hover play/pause on videos
    $(document).on('mouseenter', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.play().catch(function(){}); }
    });
    $(document).on('mouseleave', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.pause(); v.currentTime = 0; }
    });

    // OVERVIEW 이미지 마우스 오버 시 호버한 이미지 옆에만 설명 툴팁 표시 (car-classification)
    $(document).on('mouseenter', '.proj-hover-desc-item', function(){
      var desc = $(this).data('desc') || '';
      var tooltip = $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip');
      if(!tooltip.length || !desc) return;
      var r = this.getBoundingClientRect();
      var pad = 16;
      var ttW = 280;
      var ttH = 80;
      var left = r.right + pad;
      var top = r.top + (r.height / 2) - (ttH / 2);
      if(left + ttW > window.innerWidth) left = r.left - pad - ttW;
      if(top < 12) top = 12;
      if(top + ttH > window.innerHeight - 12) top = window.innerHeight - ttH - 12;
      tooltip.css({ left: left + 'px', top: top + 'px' }).text(desc).addClass('is-visible').attr('aria-hidden', 'false');
    });
    $(document).on('mouseleave', '.proj-hover-desc-item', function(){
      $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip').removeClass('is-visible').text('').attr('aria-hidden', 'true');
    });
  });
})(jQuery);
