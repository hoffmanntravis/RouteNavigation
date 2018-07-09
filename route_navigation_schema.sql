--
-- PostgreSQL database dump
--

-- Dumped from database version 10.1
-- Dumped by pg_dump version 10.3

-- Started on 2018-07-08 18:31:59

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 1 (class 3079 OID 12924)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2929 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 2 (class 3079 OID 34824)
-- Name: pg_stat_statements; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS pg_stat_statements WITH SCHEMA public;


--
-- TOC entry 2930 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pg_stat_statements; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_stat_statements IS 'track execution statistics of all SQL statements executed';


--
-- TOC entry 250 (class 1255 OID 33461)
-- Name: delete_location(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_location(p_id integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$ 
BEGIN
DELETE FROM location where id = p_id; 
RETURN 1;
END;
$$;


ALTER FUNCTION public.delete_location(p_id integer) OWNER TO postgres;

--
-- TOC entry 268 (class 1255 OID 33462)
-- Name: delete_vehicle(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_vehicle(p_id integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$ 
BEGIN
DELETE FROM vehicle where id = p_id; 
RETURN 1;
END;
$$;


ALTER FUNCTION public.delete_vehicle(p_id integer) OWNER TO postgres;

--
-- TOC entry 224 (class 1255 OID 33597)
-- Name: insert_location(integer, integer, double precision, double precision, double precision, double precision, double precision, double precision, interval, interval); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_location(p_origin_location_id integer DEFAULT NULL::integer, p_minimum_days_until_pickup integer DEFAULT NULL::integer, p_matrix_priority_multiplier double precision DEFAULT NULL::double precision, p_matrix_days_until_due_exponent double precision DEFAULT NULL::double precision, p_matrix_distance_from_source double precision DEFAULT NULL::double precision, p_matrix_overdue_multiplier double precision DEFAULT NULL::double precision, p_current_fill_level_error_margin double precision DEFAULT NULL::double precision, p_route_max_hours double precision DEFAULT NULL::double precision, p_oil_pickup_average_duration interval DEFAULT NULL::interval, p_grease_pickup_average_duration interval DEFAULT NULL::interval) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO config
           (origin_location_id,
			minimum_days_until_pickup,
			matrix_priority_multiplier,
			matrix_days_until_due_exponent,
			matrix_distance_from_source,
			matrix_overdue_multiplier,
			current_fill_level_error_margin,
			route_max_hours,
			oil_pickup_average_duration,
			grease_pickup_average_duration)
     VALUES
           (p_origin_location_id,
			p_minimum_days_until_pickup,
			p_matrix_priority_multiplier,
			p_matrix_days_until_due_exponent,
			p_matrix_distance_from_source,
			p_matrix_overdue_multiplier,
			p_current_fill_level_error_margin,
			p_route_max_hours,
			p_oil_pickup_average_duration,
			p_grease_pickup_average_duration);
     		RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_location(p_origin_location_id integer, p_minimum_days_until_pickup integer, p_matrix_priority_multiplier double precision, p_matrix_days_until_due_exponent double precision, p_matrix_distance_from_source double precision, p_matrix_overdue_multiplier double precision, p_current_fill_level_error_margin double precision, p_route_max_hours double precision, p_oil_pickup_average_duration interval, p_grease_pickup_average_duration interval) OWNER TO postgres;

--
-- TOC entry 215 (class 1255 OID 33463)
-- Name: insert_location(integer, timestamp with time zone, integer, character varying, character varying, double precision, double precision, double precision, character varying, character varying, integer, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_location(p_client_priority integer DEFAULT 1, p_last_visited timestamp with time zone DEFAULT NULL::timestamp with time zone, p_pickup_interval_days integer DEFAULT 30, p_address character varying DEFAULT NULL::character varying, p_location_name character varying DEFAULT NULL::character varying, p_capacity_gallons double precision DEFAULT NULL::double precision, p_days_until_due double precision DEFAULT NULL::double precision, p_matrix_weight double precision DEFAULT NULL::double precision, p_contact_name character varying DEFAULT NULL::character varying, p_contact_email character varying DEFAULT NULL::character varying, p_vehicle_size integer DEFAULT 10, p_distance_from_source double precision DEFAULT NULL::double precision) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO location
           (id
           ,client_priority
           ,last_visited
           ,pickup_interval_days
           ,address
           ,location_name
           ,capacity_gallons
           ,days_until_due
           ,matrix_weight
           ,contact_name
           ,contact_email
           ,vehicle_size
           ,distance_from_source)
     VALUES
           (DEFAULT
           ,p_client_priority
           ,p_last_visited
           ,p_pickup_interval_days
           ,p_address
           ,p_location_name
           ,p_capacity_gallons
           ,p_days_until_due
           ,p_matrix_weight
           ,p_contact_name
           ,p_contact_email
           ,p_vehicle_size
           ,p_distance_from_source);
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_location(p_client_priority integer, p_last_visited timestamp with time zone, p_pickup_interval_days integer, p_address character varying, p_location_name character varying, p_capacity_gallons double precision, p_days_until_due double precision, p_matrix_weight double precision, p_contact_name character varying, p_contact_email character varying, p_vehicle_size integer, p_distance_from_source double precision) OWNER TO postgres;

--
-- TOC entry 221 (class 1255 OID 33464)
-- Name: insert_route(integer, interval, integer, integer, timestamp with time zone, double precision, integer, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_route(p_batch_id integer, p_total_time interval DEFAULT NULL::interval, p_origin_location_id integer DEFAULT NULL::integer, p_destination_location_id integer DEFAULT NULL::integer, p_route_date timestamp with time zone DEFAULT NULL::timestamp with time zone, p_distance_miles double precision DEFAULT NULL::double precision, p_vehicle_id integer DEFAULT NULL::integer, p_maps_url character varying DEFAULT NULL::character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO route
           (id
           ,batch_id
           ,total_time
           ,origin_location_id
           ,destination_location_id
           ,route_date
           ,distance_miles
           ,vehicle_id
           ,maps_url
           )
     VALUES
           (DEFAULT
           ,p_batch_id
           ,p_total_time
           ,p_origin_location_id
           ,p_destination_location_id
           ,p_route_date
           ,p_distance_miles
           ,p_vehicle_id
           ,p_maps_url
           );
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_route(p_batch_id integer, p_total_time interval, p_origin_location_id integer, p_destination_location_id integer, p_route_date timestamp with time zone, p_distance_miles double precision, p_vehicle_id integer, p_maps_url character varying) OWNER TO postgres;

--
-- TOC entry 232 (class 1255 OID 70007)
-- Name: insert_route(integer, interval, integer, integer, timestamp with time zone, double precision, integer, character varying, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_route(p_batch_id integer, p_total_time interval DEFAULT NULL::interval, p_origin_location_id integer DEFAULT NULL::integer, p_destination_location_id integer DEFAULT NULL::integer, p_route_date timestamp with time zone DEFAULT NULL::timestamp with time zone, p_distance_miles double precision DEFAULT NULL::double precision, p_vehicle_id integer DEFAULT NULL::integer, p_maps_url character varying DEFAULT NULL::character varying, p_average_location_distance_miles double precision DEFAULT NULL::double precision) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO route
           (id
           ,batch_id
           ,total_time
           ,origin_location_id
           ,destination_location_id
           ,route_date
           ,distance_miles
           ,vehicle_id
           ,maps_url
		   ,average_location_distance_miles
           )
     VALUES
           (DEFAULT
           ,p_batch_id
           ,p_total_time
           ,p_origin_location_id
           ,p_destination_location_id
           ,p_route_date
           ,p_distance_miles
           ,p_vehicle_id
           ,p_maps_url
		   ,p_average_location_distance_miles
           );
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_route(p_batch_id integer, p_total_time interval, p_origin_location_id integer, p_destination_location_id integer, p_route_date timestamp with time zone, p_distance_miles double precision, p_vehicle_id integer, p_maps_url character varying, p_average_location_distance_miles double precision) OWNER TO postgres;

--
-- TOC entry 235 (class 1255 OID 33465)
-- Name: insert_route_batch(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_route_batch() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO route_batch
           (
           id
           ,date_started
           ,date_completed
           )
     VALUES
           (
           DEFAULT
           ,DEFAULT    
		   ,NULL::timestamp with time zone
           );
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_route_batch() OWNER TO postgres;

--
-- TOC entry 267 (class 1255 OID 33742)
-- Name: insert_route_location(integer, integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_route_location(p_route_id integer DEFAULT NULL::integer, p_location_id integer DEFAULT NULL::integer, p_insert_order integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO route_location
           (
           route_id
           ,location_id
           ,insert_order

           )
     VALUES
           (
           p_route_id
           ,p_location_id
           ,p_insert_order

           );
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_route_location(p_route_id integer, p_location_id integer, p_insert_order integer) OWNER TO postgres;

--
-- TOC entry 256 (class 1255 OID 33467)
-- Name: insert_vehicle(character varying, character varying, double precision, boolean, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_vehicle(p_name character varying DEFAULT NULL::character varying, p_model character varying DEFAULT NULL::character varying, p_capacity_gallons double precision DEFAULT NULL::double precision, p_operational boolean DEFAULT true, p_physical_size integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO vehicle
           (id
           ,name
           ,model
           ,capacity_gallons
           ,operational
           ,physical_size)
     VALUES
           (DEFAULT
           ,p_name
           ,p_model
           ,p_capacity_gallons
           ,p_operational
           ,p_physical_size);
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_vehicle(p_name character varying, p_model character varying, p_capacity_gallons double precision, p_operational boolean, p_physical_size integer) OWNER TO postgres;

--
-- TOC entry 236 (class 1255 OID 33468)
-- Name: select_address_by_coordinates(double precision, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_address_by_coordinates(p_lat double precision, p_lng double precision) RETURNS character varying
    LANGUAGE plpgsql
    AS $$

BEGIN    
		RETURN 
        (SELECT address from location
           WHERE coordinates_latitude = p_lat and coordinates_longitude = p_lng
               limit 1);
END;

$$;


ALTER FUNCTION public.select_address_by_coordinates(p_lat double precision, p_lng double precision) OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 197 (class 1259 OID 33469)
-- Name: config; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.config (
    id integer NOT NULL,
    matrix_priority_multiplier double precision,
    matrix_days_until_due_exponent double precision,
    matrix_distance_from_source double precision,
    current_fill_level_error_margin double precision,
    route_max_hours double precision,
    minimum_days_until_pickup integer DEFAULT 0,
    matrix_overdue_multiplier double precision,
    oil_pickup_average_duration interval,
    grease_pickup_average_duration interval,
    origin_location_id integer,
    google_api_key character varying,
    google_directions_maps_url character varying,
    google_api_illegal_characters character(1)[],
    maximum_days_overdue integer,
    route_distance_max_miles double precision
);


ALTER TABLE public.config OWNER TO postgres;

--
-- TOC entry 261 (class 1255 OID 33473)
-- Name: select_config(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_config() RETURNS SETOF public.config
    LANGUAGE sql
    AS $$

	SELECT * FROM config;

$$;


ALTER FUNCTION public.select_config() OWNER TO postgres;

--
-- TOC entry 248 (class 1255 OID 33474)
-- Name: select_days_until_due(date, numeric); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_days_until_due(p_last_visited date, p_pickup_interval_days numeric) RETURNS numeric
    LANGUAGE sql
    AS $$

select round((p_pickup_interval_days - EXTRACT(days FROM(now() - p_last_visited))::numeric),2);

$$;


ALTER FUNCTION public.select_days_until_due(p_last_visited date, p_pickup_interval_days numeric) OWNER TO postgres;

--
-- TOC entry 198 (class 1259 OID 33475)
-- Name: features; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.features (
    id integer NOT NULL,
    feature_name character varying NOT NULL,
    enabled boolean NOT NULL
);


ALTER TABLE public.features OWNER TO postgres;

--
-- TOC entry 265 (class 1255 OID 33481)
-- Name: select_features(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_features() RETURNS SETOF public.features
    LANGUAGE sql
    AS $$

	SELECT * FROM features;

$$;


ALTER FUNCTION public.select_features() OWNER TO postgres;

--
-- TOC entry 223 (class 1255 OID 33482)
-- Name: select_highest_priority_location(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_highest_priority_location() RETURNS integer
    LANGUAGE sql
    AS $$
select 1 from location;
$$;


ALTER FUNCTION public.select_highest_priority_location() OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 33483)
-- Name: location_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.location_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.location_id_seq OWNER TO postgres;

--
-- TOC entry 200 (class 1259 OID 33485)
-- Name: location; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location (
    id integer DEFAULT nextval('public.location_id_seq'::regclass) NOT NULL,
    last_visited date,
    client_priority integer DEFAULT 1 NOT NULL,
    address character varying NOT NULL,
    location_name character varying,
    capacity_gallons double precision,
    coordinates_latitude double precision,
    coordinates_longitude double precision,
    days_until_due double precision,
    pickup_interval_days integer DEFAULT 30,
    matrix_weight double precision,
    distance_from_source double precision,
    contact_name character varying,
    contact_email character varying,
    vehicle_size integer DEFAULT 10,
    visit_time interval DEFAULT '00:30:00'::interval,
    CONSTRAINT "capacity_gallons ge 0" CHECK ((capacity_gallons >= (0)::double precision)),
    CONSTRAINT coordinates_latitude_range CHECK (((coordinates_latitude <= (180)::double precision) AND (coordinates_latitude >= ('-180'::integer)::double precision))),
    CONSTRAINT coordinates_longitude_range CHECK (((coordinates_latitude <= (90)::double precision) AND (coordinates_latitude >= ('-90'::integer)::double precision)))
);


ALTER TABLE public.location OWNER TO postgres;

--
-- TOC entry 222 (class 1255 OID 33497)
-- Name: select_location(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location() RETURNS SETOF public.location
    LANGUAGE sql
    AS $$

	SELECT * FROM location order by id;

$$;


ALTER FUNCTION public.select_location() OWNER TO postgres;

--
-- TOC entry 244 (class 1255 OID 33498)
-- Name: select_location_by_address(character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_by_address(p_address character varying) RETURNS SETOF public.location
    LANGUAGE plpgsql
    AS $_$

BEGIN
		RETURN QUERY EXECUTE format ('SELECT * FROM location where address = ' || $1);

END;

$_$;


ALTER FUNCTION public.select_location_by_address(p_address character varying) OWNER TO postgres;

--
-- TOC entry 243 (class 1255 OID 33499)
-- Name: select_location_by_id(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_by_id(p_id integer) RETURNS SETOF public.location
    LANGUAGE plpgsql
    AS $_$

BEGIN
		RETURN QUERY EXECUTE format ('SELECT * FROM location where id = ' || $1);

END;

$_$;


ALTER FUNCTION public.select_location_by_id(p_id integer) OWNER TO postgres;

--
-- TOC entry 252 (class 1255 OID 33500)
-- Name: select_location_with_filter(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_with_filter(p_column_name character varying DEFAULT 'location_name'::character varying, p_filter_string character varying DEFAULT NULL::character varying) RETURNS SETOF public.location
    LANGUAGE plpgsql
    AS $_$

BEGIN
	IF p_filter_string is null
    THEN
		RETURN QUERY EXECUTE format ('SELECT * FROM location order by ' || $1);
    ELSE
		RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $1 );
	END IF;

END;

$_$;


ALTER FUNCTION public.select_location_with_filter(p_column_name character varying, p_filter_string character varying) OWNER TO postgres;

--
-- TOC entry 257 (class 1255 OID 70155)
-- Name: select_location_with_filter(character varying, character varying, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_with_filter(p_column_name character varying DEFAULT 'location_name'::character varying, p_filter_string character varying DEFAULT NULL::character varying, p_ascending boolean DEFAULT NULL::boolean) RETURNS SETOF public.location
    LANGUAGE plpgsql
    AS $_$

BEGIN
	IF p_ascending is not false
		THEN
			IF p_filter_string is null
				THEN
					RETURN QUERY EXECUTE format ('SELECT * FROM location order by ' || $1);
				ELSE
					RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $1);
			END IF;
		
		ELSE
			IF p_filter_string is null
				THEN
					RETURN QUERY EXECUTE format ('SELECT * FROM location order by ' || $1 || ' desc');
				ELSE
					RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $1 || ' desc');
				END IF;
	END IF;
	

END;

$_$;


ALTER FUNCTION public.select_location_with_filter(p_column_name character varying, p_filter_string character varying, p_ascending boolean) OWNER TO postgres;

--
-- TOC entry 227 (class 1255 OID 33501)
-- Name: select_next_location_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_location_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM location_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_location_id() OWNER TO postgres;

--
-- TOC entry 245 (class 1255 OID 33502)
-- Name: select_next_route_batch_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_route_batch_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM route_batch_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_route_batch_id() OWNER TO postgres;

--
-- TOC entry 238 (class 1255 OID 33503)
-- Name: select_next_route_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_route_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM route_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_route_id() OWNER TO postgres;

--
-- TOC entry 201 (class 1259 OID 33504)
-- Name: route_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.route_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.route_id_seq OWNER TO postgres;

--
-- TOC entry 202 (class 1259 OID 33506)
-- Name: route; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.route (
    id integer DEFAULT nextval('public.route_id_seq'::regclass) NOT NULL,
    origin_location_id integer,
    route_date timestamp with time zone,
    distance_miles double precision,
    total_time interval,
    destination_location_id integer,
    maps_url character varying,
    vehicle_id integer,
    date_calculated timestamp with time zone DEFAULT now(),
    batch_id integer,
    average_location_distance_miles double precision
);


ALTER TABLE public.route OWNER TO postgres;

--
-- TOC entry 231 (class 1255 OID 33514)
-- Name: select_route(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route() RETURNS SETOF public.route
    LANGUAGE sql
    AS $$

	SELECT * FROM route order by id;

$$;


ALTER FUNCTION public.select_route() OWNER TO postgres;

--
-- TOC entry 204 (class 1259 OID 33525)
-- Name: route_batch_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.route_batch_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.route_batch_id_seq OWNER TO postgres;

--
-- TOC entry 205 (class 1259 OID 33527)
-- Name: route_batch; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.route_batch (
    id integer DEFAULT nextval('public.route_batch_id_seq'::regclass) NOT NULL,
    date_started timestamp with time zone DEFAULT now(),
    date_completed timestamp with time zone,
    calculation_time interval,
    locations_intake_count integer,
    locations_processed_count integer,
    total_distance_miles double precision,
    total_time interval,
    locations_orphaned_count integer,
    average_route_distance_miles double precision,
    route_distance_std_dev double precision DEFAULT 0
);


ALTER TABLE public.route_batch OWNER TO postgres;

--
-- TOC entry 262 (class 1255 OID 35061)
-- Name: select_route_batch(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_batch() RETURNS SETOF public.route_batch
    LANGUAGE sql
    AS $$

	SELECT * FROM route_batch
    order by date_started desc NULLS LAST;

$$;


ALTER FUNCTION public.select_route_batch() OWNER TO postgres;

--
-- TOC entry 258 (class 1255 OID 33515)
-- Name: select_route_by_id(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_by_id(p_id integer) RETURNS SETOF public.route
    LANGUAGE plpgsql
    AS $_$

BEGIN
		RETURN QUERY EXECUTE format ('SELECT * FROM route where id = ' || $1);

END;

$_$;


ALTER FUNCTION public.select_route_by_id(p_id integer) OWNER TO postgres;

--
-- TOC entry 203 (class 1259 OID 33516)
-- Name: route_location; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.route_location (
    route_id integer,
    location_id integer,
    insert_order integer
);


ALTER TABLE public.route_location OWNER TO postgres;

--
-- TOC entry 212 (class 1259 OID 78374)
-- Name: route_details; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.route_details AS
 SELECT r.id AS route_id,
    l.id AS location_id,
    l.location_name,
    l.client_priority,
    l.address,
    l.last_visited,
    l.days_until_due,
    l.matrix_weight,
    l.coordinates_latitude,
    l.coordinates_longitude,
    r.route_date,
    rl.insert_order
   FROM ((public.route_location rl
     JOIN public.location l ON ((rl.location_id = l.id)))
     JOIN public.route r ON ((rl.route_id = r.id)))
  ORDER BY rl.insert_order;


ALTER TABLE public.route_details OWNER TO postgres;

--
-- TOC entry 242 (class 1255 OID 78379)
-- Name: select_route_details(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_details(p_route_id integer DEFAULT NULL::integer) RETURNS SETOF public.route_details
    LANGUAGE plpgsql
    AS $$

BEGIN
	IF p_route_id is null
    THEN
    	RETURN QUERY EXECUTE format ('SELECT * FROM route_details');
    ELSE
		RETURN QUERY EXECUTE format ('SELECT * FROM route_details where route_id = ' || p_route_id);
	END IF;
END;

$$;


ALTER FUNCTION public.select_route_details(p_route_id integer) OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 33532)
-- Name: vehicle_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.vehicle_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.vehicle_id_seq OWNER TO postgres;

--
-- TOC entry 207 (class 1259 OID 33534)
-- Name: vehicle; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.vehicle (
    id integer DEFAULT nextval('public.vehicle_id_seq'::regclass) NOT NULL,
    name character varying,
    model character varying,
    capacity_gallons double precision,
    physical_size integer DEFAULT 1,
    operational boolean DEFAULT true
);


ALTER TABLE public.vehicle OWNER TO postgres;

--
-- TOC entry 211 (class 1259 OID 70010)
-- Name: route_information; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.route_information AS
 SELECT r.id,
    r.route_date,
    r.total_time,
    r.distance_miles,
    r.average_location_distance_miles,
    l_origin.id AS origin_location_id,
    l_origin.address AS origin_location_address,
    l_destination.id AS destination_location_id,
    l_destination.address AS destination_location_address,
    v.id AS vehicle_id,
    v.name AS vehicle_name,
    v.model AS vehicle_model,
    r.maps_url,
    rb.id AS batch_id
   FROM ((((public.route r
     JOIN public.location l_origin ON ((r.origin_location_id = l_origin.id)))
     JOIN public.location l_destination ON ((r.destination_location_id = l_destination.id)))
     JOIN public.vehicle v ON ((r.vehicle_id = v.id)))
     JOIN public.route_batch rb ON ((rb.id = r.batch_id)))
  ORDER BY r.route_date;


ALTER TABLE public.route_information OWNER TO postgres;

--
-- TOC entry 254 (class 1255 OID 70015)
-- Name: select_route_information(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_information() RETURNS SETOF public.route_information
    LANGUAGE sql
    AS $$

	SELECT * FROM route_information
    where batch_id = (select id from route_batch order by id desc limit 1)
    order by route_date asc;

$$;


ALTER FUNCTION public.select_route_information() OWNER TO postgres;

--
-- TOC entry 260 (class 1255 OID 33549)
-- Name: select_route_with_filter(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_with_filter(p_column_name character varying DEFAULT 'id'::character varying, p_filter_string character varying DEFAULT NULL::character varying) RETURNS SETOF public.route
    LANGUAGE plpgsql
    AS $_$

BEGIN
	IF p_filter_string is null
    THEN
		RETURN QUERY EXECUTE format ('SELECT * FROM route order by ' || $1);
    ELSE
		RETURN QUERY EXECUTE format ('Select * FROM route where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $1 );
	END IF;

END;

$_$;


ALTER FUNCTION public.select_route_with_filter(p_column_name character varying, p_filter_string character varying) OWNER TO postgres;

--
-- TOC entry 217 (class 1255 OID 33550)
-- Name: select_vehicle(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_vehicle() RETURNS SETOF public.vehicle
    LANGUAGE sql
    AS $$

	SELECT * FROM vehicle order by id;


$$;


ALTER FUNCTION public.select_vehicle() OWNER TO postgres;

--
-- TOC entry 234 (class 1255 OID 33551)
-- Name: select_vehicle_with_filter(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_vehicle_with_filter(p_column_name character varying DEFAULT 'name'::character varying, p_filter_string character varying DEFAULT NULL::character varying) RETURNS SETOF public.vehicle
    LANGUAGE plpgsql
    AS $_$

BEGIN
	IF p_filter_string is null
    THEN
		RETURN QUERY EXECUTE format ('SELECT * FROM vehicle order by ' || $1);
    ELSE
		RETURN QUERY EXECUTE format ('Select * FROM vehicle where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $2 );
	END IF;

END;

$_$;


ALTER FUNCTION public.select_vehicle_with_filter(p_column_name character varying, p_filter_string character varying) OWNER TO postgres;

--
-- TOC entry 241 (class 1255 OID 33757)
-- Name: update_days_until_due(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_days_until_due() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE location
SET days_until_due = ROUND((pickup_interval_days - EXTRACT(epoch FROM(now() - last_visited))/86400)::numeric,2);

RETURN true;
END;

$$;


ALTER FUNCTION public.update_days_until_due() OWNER TO postgres;

--
-- TOC entry 219 (class 1255 OID 33554)
-- Name: update_features(character varying, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_features(p_feature_name character varying, p_enabled boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE features   
           SET 
           enabled = COALESCE(p_enabled, enabled)
           WHERE feature_name = p_feature_name;
           return 1;

           
END;

$$;


ALTER FUNCTION public.update_features(p_feature_name character varying, p_enabled boolean) OWNER TO postgres;

--
-- TOC entry 249 (class 1255 OID 33555)
-- Name: update_location(integer, integer, timestamp with time zone, integer, character varying, character varying, integer, double precision, double precision, double precision, double precision, double precision, character varying, character varying, integer, interval); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_location(p_id integer, p_client_priority integer DEFAULT NULL::integer, p_last_visited timestamp with time zone DEFAULT NULL::timestamp with time zone, p_pickup_interval_days integer DEFAULT NULL::integer, p_address character varying DEFAULT NULL::character varying, p_location_name character varying DEFAULT NULL::character varying, p_capacity_gallons integer DEFAULT NULL::integer, p_coordinates_latitude double precision DEFAULT NULL::double precision, p_coordinates_longitude double precision DEFAULT NULL::double precision, p_days_until_due double precision DEFAULT NULL::double precision, p_matrix_weight double precision DEFAULT NULL::double precision, p_distance_from_source double precision DEFAULT NULL::double precision, p_contact_name character varying DEFAULT NULL::character varying, p_contact_email character varying DEFAULT NULL::character varying, p_vehicle_size integer DEFAULT NULL::integer, p_visit_time interval DEFAULT NULL::interval) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE location	   
           SET 
            client_priority = COALESCE(p_client_priority, client_priority)
           ,last_visited = COALESCE(p_last_visited, last_visited)
           ,pickup_interval_days = COALESCE(p_pickup_interval_days, pickup_interval_days)
           ,address = COALESCE(p_address,address)
           ,location_name = COALESCE(p_location_name,location_name)
           ,capacity_gallons = COALESCE(p_capacity_gallons,capacity_gallons)
           ,coordinates_latitude = COALESCE(p_coordinates_latitude,coordinates_latitude)
	       ,coordinates_longitude = COALESCE(p_coordinates_longitude,coordinates_longitude)
           ,days_until_due = COALESCE(p_days_until_due,days_until_due) 
           ,matrix_weight = COALESCE(p_matrix_weight,matrix_weight) 
           ,distance_from_source = COALESCE(p_distance_from_source,distance_from_source) 
           ,contact_name = COALESCE(p_contact_name,contact_name)
           ,contact_email = COALESCE(p_contact_email,contact_email)
           ,vehicle_size = COALESCE(p_vehicle_size,vehicle_size)
           ,visit_time = COALESCE(p_visit_time,visit_time)                       

           WHERE id = p_id;
           return 1;

END;

$$;


ALTER FUNCTION public.update_location(p_id integer, p_client_priority integer, p_last_visited timestamp with time zone, p_pickup_interval_days integer, p_address character varying, p_location_name character varying, p_capacity_gallons integer, p_coordinates_latitude double precision, p_coordinates_longitude double precision, p_days_until_due double precision, p_matrix_weight double precision, p_distance_from_source double precision, p_contact_name character varying, p_contact_email character varying, p_vehicle_size integer, p_visit_time interval) OWNER TO postgres;

--
-- TOC entry 218 (class 1255 OID 33692)
-- Name: update_maps_url(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_maps_url(p_address integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$


BEGIN
--UPDATE route set maps_url = 
select 
    (select google_directions_maps_url from config limit 1)  || (select string_agg(address,'+') from route_details where address = p_address) || (select google_api_key from config limit 1);

     Return 1;      
END;

$$;


ALTER FUNCTION public.update_maps_url(p_address integer) OWNER TO postgres;

--
-- TOC entry 239 (class 1255 OID 33693)
-- Name: update_maps_url(character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_maps_url(p_address character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$


BEGIN
--UPDATE route set maps_url = 
select 
    (select google_directions_maps_url from config limit 1)  || (select string_agg(address,'+') from route_details where address = p_address) || (select google_api_key from config limit 1);

     Return 1;      
END;

$$;


ALTER FUNCTION public.update_maps_url(p_address character varying) OWNER TO postgres;

--
-- TOC entry 255 (class 1255 OID 33758)
-- Name: update_maps_urls(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_maps_urls() RETURNS trigger
    LANGUAGE plpgsql
    AS $$

BEGIN
        UPDATE route
		SET maps_url = 
               (select (select c.google_directions_maps_url from config as c limit 1)
               || REGEXP_REPLACE(
               replace(
               '?api=1&origin=' 
               ||(select rd.address from route_details as rd where rd.route_id = rdsub.route_id and rd.insert_order = 0)
               || '&waypoints=' 
               ||(select string_agg(rd.address,'|') from route_details as rd where rd.route_id = rdsub.route_id and rd.insert_order != 0 
                  and rd.insert_order < (select max(rd.insert_order) from route_details as rd where rd.route_id = rdsub.route_id)) 
    		   || '&destination='
    		   ||(select rd.address from route_details as rd where rd.insert_order = (select max(rd.insert_order) from route_details as rd where rd.route_id = rdsub.route_id) and rd.route_id = rdsub.route_id)
        	   || '&apikey='
               ||(select c.google_api_key from config as c limit 1)
               ,' ','+')
               ,'[\#\. ]',''))
        FROM 
        (SELECT route_id,address,insert_order from route_details where location_id = OLD.id) as rdsub
        where id = rdsub.route_id;
               return NEW;
               
               
END;

$$;


ALTER FUNCTION public.update_maps_urls() OWNER TO postgres;

--
-- TOC entry 263 (class 1255 OID 33556)
-- Name: update_route_batch_calculation_time(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_route_batch_calculation_time() RETURNS trigger
    LANGUAGE plpgsql
    AS $$BEGIN
UPDATE route_batch   
           set calculation_time = (date_completed - date_started)::interval 
           where date_completed is not null and date_started is not null and id=NEW.id;
           return NEW;         
END;

$$;


ALTER FUNCTION public.update_route_batch_calculation_time() OWNER TO postgres;

--
-- TOC entry 253 (class 1255 OID 69994)
-- Name: update_route_batch_metadata(integer, integer, integer, integer, double precision, interval, double precision, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_route_batch_metadata(p_id integer, p_locations_intake_count integer, p_locations_processed_count integer, p_locations_orphaned_count integer, p_total_distance_miles double precision, p_total_time interval, p_average_route_distance_miles double precision, p_route_distance_std_dev double precision) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE route_batch   
   SET 
   date_completed = now()::timestamp with time zone,
   locations_intake_count = p_locations_intake_count,
   locations_processed_count = p_locations_processed_count,
   locations_orphaned_count = p_locations_orphaned_count,
   total_distance_miles = p_total_distance_miles,
   total_time = p_total_time,
   average_route_distance_miles = p_average_route_distance_miles,
   route_distance_std_dev = p_route_distance_std_dev
   WHERE id = p_id;
   return 1;
END;

$$;


ALTER FUNCTION public.update_route_batch_metadata(p_id integer, p_locations_intake_count integer, p_locations_processed_count integer, p_locations_orphaned_count integer, p_total_distance_miles double precision, p_total_time interval, p_average_route_distance_miles double precision, p_route_distance_std_dev double precision) OWNER TO postgres;

--
-- TOC entry 228 (class 1255 OID 33663)
-- Name: update_route_map_url(character varying, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_route_map_url(p_route_id character varying, p_maps_url boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE maps_url   
           SET 
           maps_url = COALESCE(p_maps_url, maps_url)
           WHERE route_id = p_route_id;
           return 1;

           
END;

$$;


ALTER FUNCTION public.update_route_map_url(p_route_id character varying, p_maps_url boolean) OWNER TO postgres;

--
-- TOC entry 246 (class 1255 OID 33558)
-- Name: update_vehicle(integer, character varying, character varying, double precision, boolean, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_vehicle(p_id integer, p_name character varying DEFAULT NULL::character varying, p_model character varying DEFAULT NULL::character varying, p_capacity_gallons double precision DEFAULT NULL::double precision, p_operational boolean DEFAULT NULL::boolean, p_physical_size integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE vehicle   
           SET 
            name = COALESCE(p_name, name)
           ,model = COALESCE(p_model, model)
           ,capacity_gallons = COALESCE(p_capacity_gallons, capacity_gallons)
           ,operational = COALESCE(p_operational,operational)
           ,physical_size = COALESCE(p_physical_size,physical_size)
           WHERE id = p_id;
           return 1;

           
END;

$$;


ALTER FUNCTION public.update_vehicle(p_id integer, p_name character varying, p_model character varying, p_capacity_gallons double precision, p_operational boolean, p_physical_size integer) OWNER TO postgres;

--
-- TOC entry 214 (class 1255 OID 33608)
-- Name: upsert_api_metadata(date, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.upsert_api_metadata(p_call_date date, p_api_call_count integer DEFAULT 1) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO api_metadata
           (id
           ,call_date
           ,api_call_count)
     VALUES
           (DEFAULT
           ,p_call_date
           ,p_api_call_count)
    ON
    CONFLICT (call_date)
	DO UPDATE 
    	SET api_call_count = api_metadata.api_call_count + 1;
     RETURN 1;
END;

$$;


ALTER FUNCTION public.upsert_api_metadata(p_call_date date, p_api_call_count integer) OWNER TO postgres;

--
-- TOC entry 225 (class 1255 OID 70051)
-- Name: upsert_config(integer, integer, double precision, double precision, double precision, double precision, double precision, double precision, double precision, interval, integer, interval, character varying, character varying, character[]); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.upsert_config(p_origin_location_id integer DEFAULT NULL::integer, p_minimum_days_until_pickup integer DEFAULT NULL::integer, p_matrix_priority_multiplier double precision DEFAULT NULL::double precision, p_matrix_days_until_due_exponent double precision DEFAULT NULL::double precision, p_matrix_distance_from_source double precision DEFAULT NULL::double precision, p_matrix_overdue_multiplier double precision DEFAULT NULL::double precision, p_current_fill_level_error_margin double precision DEFAULT NULL::double precision, p_route_max_hours double precision DEFAULT NULL::double precision, p_route_distance_max_miles double precision DEFAULT NULL::double precision, p_oil_pickup_average_duration interval DEFAULT NULL::interval, p_maximum_days_overdue integer DEFAULT NULL::integer, p_grease_pickup_average_duration interval DEFAULT NULL::interval, p_google_directions_maps_url character varying DEFAULT NULL::character varying, p_google_api_key character varying DEFAULT NULL::character varying, p_google_api_illegal_characters character[] DEFAULT NULL::character(1)[]) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO config 
(
    id
,origin_location_id
,minimum_days_until_pickup
,matrix_priority_multiplier
,matrix_days_until_due_exponent
,matrix_distance_from_source
,matrix_overdue_multiplier
,current_fill_level_error_margin
,route_max_hours
,route_distance_max_miles
,oil_pickup_average_duration
,maximum_days_overdue
,grease_pickup_average_duration
,google_directions_maps_url
,google_api_key 
,google_api_illegal_characters
)
VALUES
(
1
,p_origin_location_id
,p_minimum_days_until_pickup
,p_matrix_priority_multiplier
,p_matrix_days_until_due_exponent
,p_matrix_distance_from_source
,p_matrix_overdue_multiplier
,p_current_fill_level_error_margin
,p_route_max_hours
,p_route_distance_max_miles
,p_oil_pickup_average_duration
,p_maximum_days_overdue
,p_grease_pickup_average_duration
,p_google_directions_maps_url
,p_google_api_key
,p_google_api_illegal_characters)
    ON CONFLICT (id)
DO UPDATE    
           SET 
           origin_location_id = COALESCE(p_origin_location_id, config.origin_location_id),
           minimum_days_until_pickup = COALESCE(p_minimum_days_until_pickup, config.minimum_days_until_pickup),
           matrix_priority_multiplier = COALESCE(p_matrix_priority_multiplier, config.matrix_priority_multiplier),
           matrix_days_until_due_exponent = COALESCE(p_matrix_days_until_due_exponent, config.matrix_days_until_due_exponent),
           matrix_distance_from_source = COALESCE(p_matrix_distance_from_source, config.matrix_distance_from_source),
           matrix_overdue_multiplier = COALESCE(p_matrix_overdue_multiplier, config.matrix_overdue_multiplier),
           current_fill_level_error_margin = COALESCE(p_current_fill_level_error_margin, config.current_fill_level_error_margin),
    	   route_max_hours = COALESCE(p_route_max_hours, config.route_max_hours),
		   route_distance_max_miles = COALESCE(p_route_distance_max_miles, config.route_distance_max_miles),
           oil_pickup_average_duration = COALESCE(p_oil_pickup_average_duration, config.oil_pickup_average_duration),
           maximum_days_overdue = COALESCE(p_maximum_days_overdue, config.maximum_days_overdue),
           grease_pickup_average_duration = COALESCE(p_grease_pickup_average_duration, config.grease_pickup_average_duration),
		   google_directions_maps_url = COALESCE(p_google_directions_maps_url, config.google_directions_maps_url),
		   google_api_key = COALESCE(p_google_api_key, config.google_api_key),
		   google_api_illegal_characters =COALESCE(p_google_api_illegal_characters, config.google_api_illegal_characters)

;
           return 1;

END;

$$;


ALTER FUNCTION public.upsert_config(p_origin_location_id integer, p_minimum_days_until_pickup integer, p_matrix_priority_multiplier double precision, p_matrix_days_until_due_exponent double precision, p_matrix_distance_from_source double precision, p_matrix_overdue_multiplier double precision, p_current_fill_level_error_margin double precision, p_route_max_hours double precision, p_route_distance_max_miles double precision, p_oil_pickup_average_duration interval, p_maximum_days_overdue integer, p_grease_pickup_average_duration interval, p_google_directions_maps_url character varying, p_google_api_key character varying, p_google_api_illegal_characters character[]) OWNER TO postgres;

--
-- TOC entry 208 (class 1259 OID 33560)
-- Name: api_metadata_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.api_metadata_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.api_metadata_id_seq OWNER TO postgres;

--
-- TOC entry 209 (class 1259 OID 33604)
-- Name: api_metadata; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.api_metadata (
    id integer DEFAULT nextval('public.api_metadata_id_seq'::regclass) NOT NULL,
    call_date date NOT NULL,
    api_call_count integer
);


ALTER TABLE public.api_metadata OWNER TO postgres;

--
-- TOC entry 2793 (class 2606 OID 33630)
-- Name: api_metadata call_date_unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.api_metadata
    ADD CONSTRAINT call_date_unique UNIQUE (call_date);


--
-- TOC entry 2787 (class 2606 OID 33622)
-- Name: config config_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.config
    ADD CONSTRAINT config_pkey PRIMARY KEY (id);


--
-- TOC entry 2789 (class 2606 OID 34793)
-- Name: location location_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location
    ADD CONSTRAINT location_pkey PRIMARY KEY (id);


--
-- TOC entry 2791 (class 2606 OID 34773)
-- Name: route route_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route
    ADD CONSTRAINT route_pkey PRIMARY KEY (id);


--
-- TOC entry 2797 (class 2620 OID 34286)
-- Name: route_batch update_batch_route_calculation_time; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_batch_route_calculation_time AFTER UPDATE OF date_completed ON public.route_batch FOR EACH ROW EXECUTE PROCEDURE public.update_route_batch_calculation_time();


--
-- TOC entry 2796 (class 2620 OID 33947)
-- Name: location update_maps_urls; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_maps_urls AFTER UPDATE OF address ON public.location FOR EACH ROW WHEN (((old.address)::text <> (new.address)::text)) EXECUTE PROCEDURE public.update_maps_urls();


--
-- TOC entry 2795 (class 2606 OID 34811)
-- Name: route_location location_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route_location
    ADD CONSTRAINT location_id FOREIGN KEY (location_id) REFERENCES public.location(id);


--
-- TOC entry 2794 (class 2606 OID 34806)
-- Name: route_location route_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route_location
    ADD CONSTRAINT route_id FOREIGN KEY (route_id) REFERENCES public.route(id);


-- Completed on 2018-07-08 18:32:00

--
-- PostgreSQL database dump complete
--

