---
layout: default
title: "Lightning Bug"
---
<!-- 
{{ site.posts.size }} posts found.
-->
{% assign maxPreview = 8 %}
{% for post in site.posts limit:maxPreview%}
<div id="post">
	<div class="post-date">
		<a href="{{ post.url }}">
			<div class="datebox">
				<span class="month">{{ post.date | date : "%b" }}</span>
				<span class="day">{{ post.date | date : "%d" }}</span>
				<span class="year">{{ post.date | date : "%Y" }}</span>
			</div>
		</a>
	</div>
	<h3><a href="{{ post.url }}">{{ post.title }}</a></h3>
	{% if post.excerpt.size %}
		{% capture excerpt %}{{ post.excerpt }} [Continue reading]({{ post.url }}){% endcapture %}
		{{ excerpt | markdownify }}
	{% else %}
	<i>Sorry. No excerpt is available for this post. <a href="{{ post.url }}">Read the whole post.</a></i>
	{% endif %}
</div>
{% endfor %}

{% if site.posts.size > maxPreview %}
<div id="post">
	<h3>Older Posts</h3>
	<ul id="older-posts">
	{% for post in site.posts offset:maxPreview%}
      <li><span class="older-post-date">{{ post.date | date_to_string }}</span> &raquo; <a href="{{ post.url }}">{{ post.title }}</a></li>
	{% endfor %}
	</ul>
</div>
{% endif %}