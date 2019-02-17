--
-- PostgreSQL database dump
--

-- Dumped from database version 10.1
-- Dumped by pg_dump version 10.3

-- Started on 2019-02-17 02:37:17

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
-- TOC entry 2985 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 2 (class 3079 OID 174018)
-- Name: pg_stat_statements; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS pg_stat_statements WITH SCHEMA public;


--
-- TOC entry 2986 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pg_stat_statements; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_stat_statements IS 'track execution statistics of all SQL statements executed';


--
-- TOC entry 258 (class 1255 OID 182598)
-- Name: calculate_xyz_cartesian_from_gps(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.calculate_xyz_cartesian_from_gps() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
UPDATE location
		set
		cartesian_x = 3963.190592 * cos(radians(coordinates_latitude)) * cos(radians(coordinates_longitude)),
		cartesian_y = 3963.190592 * cos(radians(coordinates_latitude)) * sin(radians(coordinates_longitude)),
		cartesian_z = 3963.190592 * sin(radians(coordinates_latitude))
		where id=NEW.id;
        return NEW;     
END;
$$;


ALTER FUNCTION public.calculate_xyz_cartesian_from_gps() OWNER TO postgres;

--
-- TOC entry 273 (class 1255 OID 174025)
-- Name: delete_location(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_location(p_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
 
BEGIN
DELETE FROM location where id = p_id; 
RETURN 1;
END;

$$;


ALTER FUNCTION public.delete_location(p_id integer) OWNER TO postgres;

--
-- TOC entry 269 (class 1255 OID 174026)
-- Name: delete_location_from_route_location(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_location_from_route_location(p_route_id integer, p_location_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
 
BEGIN
DELETE FROM location where route_id = p_route_id and location_id = p_location_id; 

RETURN 1;
END;

$$;


ALTER FUNCTION public.delete_location_from_route_location(p_route_id integer, p_location_id integer) OWNER TO postgres;

--
-- TOC entry 235 (class 1255 OID 174027)
-- Name: delete_location_wildcard(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_location_wildcard(p_string integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

 
BEGIN
select * FROM location where lower(name) like  '%' + p_string + '%'; 
RETURN 1;
END;

$$;


ALTER FUNCTION public.delete_location_wildcard(p_string integer) OWNER TO postgres;

--
-- TOC entry 220 (class 1255 OID 174028)
-- Name: delete_location_wildcard(character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_location_wildcard(p_string character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

 
BEGIN
delete FROM location where lower(account) like '%' || p_string || '%';
RETURN 1;
END;

$$;


ALTER FUNCTION public.delete_location_wildcard(p_string character varying) OWNER TO postgres;

--
-- TOC entry 257 (class 1255 OID 174029)
-- Name: delete_null_route_batch(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.delete_null_route_batch(p_id integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

 
BEGIN
DELETE FROM route_batch where date_completed is null; 
RETURN 1;
END;

$$;


ALTER FUNCTION public.delete_null_route_batch(p_id integer) OWNER TO postgres;

--
-- TOC entry 250 (class 1255 OID 174030)
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
-- TOC entry 252 (class 1255 OID 174031)
-- Name: get_calc_status(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_calc_status() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
	IF (SELECT date_completed FROM route_batch 
		order by id desc limit 1) is not null
		THEN
		return true;
		ELSE
		return false;
	END IF;
END;

$$;


ALTER FUNCTION public.get_calc_status() OWNER TO postgres;

--
-- TOC entry 280 (class 1255 OID 174032)
-- Name: get_latest_completed_batch_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_latest_completed_batch_id() RETURNS bigint
    LANGUAGE plpgsql
    AS $$
BEGIN
	return (select route_batch.id FROM route_batch WHERE route_batch.date_completed IS NOT NULL ORDER BY route_batch.id DESC LIMIT 1);
END;
$$;


ALTER FUNCTION public.get_latest_completed_batch_id() OWNER TO postgres;

--
-- TOC entry 216 (class 1255 OID 174033)
-- Name: get_route_batch_cancellation_status(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_route_batch_cancellation_status() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
	RETURN cancellation_request from route_batch where id = (SELECT id FROM route_batch 
		order by id desc limit 1);
END;

$$;


ALTER FUNCTION public.get_route_batch_cancellation_status() OWNER TO postgres;

--
-- TOC entry 278 (class 1255 OID 182310)
-- Name: insert_location(integer, date, integer, time without time zone, time without time zone, character varying, character varying, double precision, double precision, character varying, character varying, integer, boolean, boolean, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_location(p_client_priority integer DEFAULT 1, p_intended_pickup_date date DEFAULT NULL::date, p_oil_pickup_schedule integer DEFAULT 30, p_grease_trap_preferred_time_start time without time zone DEFAULT NULL::time without time zone, p_grease_trap_preferred_time_end time without time zone DEFAULT NULL::time without time zone, p_address character varying DEFAULT NULL::character varying, p_account character varying DEFAULT NULL::character varying, p_oil_tank_size double precision DEFAULT NULL::double precision, p_days_until_due double precision DEFAULT NULL::double precision, p_contact_name character varying DEFAULT NULL::character varying, p_contact_email character varying DEFAULT NULL::character varying, p_vehicle_size integer DEFAULT 10, p_oil_pickup_customer boolean DEFAULT false, p_grease_trap_customer boolean DEFAULT false, p_distance_from_source double precision DEFAULT NULL::double precision) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO location
           (id
           ,client_priority
           ,intended_pickup_date
           ,oil_pickup_schedule
		   ,grease_trap_preferred_time_start
		   ,grease_trap_preferred_time_end
           ,address
           ,account
           ,oil_tank_size
           ,days_until_due
           ,contact_name
           ,contact_email
           ,vehicle_size
		   ,oil_pickup_customer
		   ,grease_trap_customer
           ,distance_from_source)
     VALUES
           (DEFAULT
           ,p_client_priority
           ,p_intended_pickup_date
           ,p_oil_pickup_schedule
		   ,p_grease_trap_preferred_time_start
		   ,p_grease_trap_preferred_time_end
           ,p_address
           ,p_account
           ,p_oil_tank_size
           ,p_days_until_due
           ,p_contact_name
           ,p_contact_email
           ,p_vehicle_size
		   ,p_oil_pickup_customer
		   ,p_grease_trap_customer
           ,p_distance_from_source);
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_location(p_client_priority integer, p_intended_pickup_date date, p_oil_pickup_schedule integer, p_grease_trap_preferred_time_start time without time zone, p_grease_trap_preferred_time_end time without time zone, p_address character varying, p_account character varying, p_oil_tank_size double precision, p_days_until_due double precision, p_contact_name character varying, p_contact_email character varying, p_vehicle_size integer, p_oil_pickup_customer boolean, p_grease_trap_customer boolean, p_distance_from_source double precision) OWNER TO postgres;

--
-- TOC entry 281 (class 1255 OID 182650)
-- Name: insert_location(integer, text, text, integer, text, text, double precision, double precision, boolean, timestamp without time zone, text, interval, interval, integer, text, boolean, double precision, integer, timestamp without time zone, integer, boolean, timestamp without time zone, integer, text, boolean, double precision, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_location(p_tracking_number integer DEFAULT NULL::integer, p_account text DEFAULT NULL::text, p_address text DEFAULT NULL::text, p_client_priority integer DEFAULT 1, p_contact_email text DEFAULT NULL::text, p_contact_name text DEFAULT NULL::text, p_days_until_due double precision DEFAULT NULL::double precision, p_distance_from_source double precision DEFAULT NULL::double precision, p_grease_trap_customer boolean DEFAULT false, p_grease_trap_pickup_next_date timestamp without time zone DEFAULT NULL::timestamp without time zone, p_grease_trap_preferred_day text DEFAULT NULL::text, p_grease_trap_preferred_time_end interval DEFAULT NULL::interval, p_grease_trap_preferred_time_start interval DEFAULT NULL::interval, p_grease_trap_schedule integer DEFAULT 30, p_grease_trap_service_notes text DEFAULT NULL::text, p_grease_trap_signature_req boolean DEFAULT NULL::boolean, p_grease_trap_size double precision DEFAULT NULL::double precision, p_grease_trap_units integer DEFAULT NULL::integer, p_intended_pickup_date timestamp without time zone DEFAULT NULL::timestamp without time zone, p_number_of_manholes integer DEFAULT NULL::integer, p_oil_pickup_customer boolean DEFAULT false, p_oil_pickup_next_date timestamp without time zone DEFAULT NULL::timestamp without time zone, p_oil_pickup_schedule integer DEFAULT 30, p_oil_pickup_service_notes text DEFAULT NULL::text, p_oil_pickup_signature_req boolean DEFAULT NULL::boolean, p_oil_tank_size double precision DEFAULT NULL::double precision, p_vehicle_size integer DEFAULT 10) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO location
           (
			id,
			tracking_number,
			account,
			address,
			client_priority,
			contact_email,
			contact_name,
			days_until_due,
			distance_from_source,
			grease_trap_customer,
			grease_trap_pickup_next_date,
			grease_trap_preferred_day,
			grease_trap_preferred_time_end,
			grease_trap_preferred_time_start,
			grease_trap_schedule,
			grease_trap_service_notes,
			grease_trap_signature_req,
			grease_trap_size,
			grease_trap_units,
			intended_pickup_date,
			number_of_manholes,
			oil_pickup_customer,
			oil_pickup_next_date,
			oil_pickup_schedule,
			oil_pickup_service_notes,
			oil_pickup_signature_req,
			oil_tank_size,
			vehicle_size)
			
		    VALUES
           (
			DEFAULT,
			p_tracking_number,
            p_account,
			p_address,
			p_client_priority,
			p_contact_email,
			p_contact_name,
			p_days_until_due,
			p_distance_from_source,
			p_grease_trap_customer,
			p_grease_trap_pickup_next_date,
			p_grease_trap_preferred_day,
			p_grease_trap_preferred_time_end,
			p_grease_trap_preferred_time_start,
			p_grease_trap_schedule,
			p_grease_trap_service_notes,
			p_grease_trap_signature_req,
			p_grease_trap_size,
			p_grease_trap_units,
			p_intended_pickup_date,
			p_number_of_manholes,
			p_oil_pickup_customer,
			p_oil_pickup_next_date,
			p_oil_pickup_schedule,
			p_oil_pickup_service_notes,
			p_oil_pickup_signature_req,
			p_oil_tank_size,
			p_vehicle_size)
			
		   ON CONFLICT (tracking_number)
		   DO UPDATE    
           SET 
		    tracking_number = COALESCE(p_tracking_number,location.tracking_number),
			account = COALESCE(p_account,location.account),
			address = COALESCE(p_address,location.address),
			client_priority = COALESCE(p_client_priority,location.client_priority),
			contact_email = COALESCE(p_contact_email,location.contact_email),
			contact_name = COALESCE(p_contact_name,location.contact_name),
			days_until_due = COALESCE(p_days_until_due,location.days_until_due),
			distance_from_source = COALESCE(p_distance_from_source,location.distance_from_source),
			grease_trap_customer = COALESCE(p_grease_trap_customer,location.grease_trap_customer),
			grease_trap_pickup_next_date = COALESCE(p_grease_trap_pickup_next_date,location.grease_trap_pickup_next_date),
			grease_trap_preferred_day = COALESCE(p_grease_trap_preferred_day,location.grease_trap_preferred_day),
			grease_trap_preferred_time_end = COALESCE(p_grease_trap_preferred_time_end::time without time zone,location.grease_trap_preferred_time_end),
			grease_trap_preferred_time_start = COALESCE(p_grease_trap_preferred_time_start::time without time zone,location.grease_trap_preferred_time_start),
			grease_trap_schedule = COALESCE(p_grease_trap_schedule,location.grease_trap_schedule),
			grease_trap_service_notes = COALESCE(p_grease_trap_service_notes,location.grease_trap_service_notes),
			grease_trap_signature_req = COALESCE(p_grease_trap_signature_req,location.grease_trap_signature_req),
			grease_trap_size = COALESCE(p_grease_trap_size,location.grease_trap_size),
			grease_trap_units = COALESCE(p_grease_trap_units,location.grease_trap_units),
			intended_pickup_date = COALESCE(p_intended_pickup_date,location.intended_pickup_date),
			number_of_manholes = COALESCE(p_number_of_manholes,location.number_of_manholes),
			oil_pickup_customer = COALESCE(p_oil_pickup_customer,location.oil_pickup_customer),
			oil_pickup_next_date = COALESCE(p_oil_pickup_next_date,location.oil_pickup_next_date),
			oil_pickup_schedule = COALESCE(p_oil_pickup_schedule,location.oil_pickup_schedule),
			oil_pickup_service_notes = COALESCE(p_oil_pickup_service_notes,location.oil_pickup_service_notes),
			oil_pickup_signature_req = COALESCE(p_oil_pickup_signature_req,location.oil_pickup_signature_req),
			oil_tank_size = COALESCE(p_oil_tank_size,location.oil_tank_size),
			vehicle_size = COALESCE(p_vehicle_size,location.vehicle_size);

     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_location(p_tracking_number integer, p_account text, p_address text, p_client_priority integer, p_contact_email text, p_contact_name text, p_days_until_due double precision, p_distance_from_source double precision, p_grease_trap_customer boolean, p_grease_trap_pickup_next_date timestamp without time zone, p_grease_trap_preferred_day text, p_grease_trap_preferred_time_end interval, p_grease_trap_preferred_time_start interval, p_grease_trap_schedule integer, p_grease_trap_service_notes text, p_grease_trap_signature_req boolean, p_grease_trap_size double precision, p_grease_trap_units integer, p_intended_pickup_date timestamp without time zone, p_number_of_manholes integer, p_oil_pickup_customer boolean, p_oil_pickup_next_date timestamp without time zone, p_oil_pickup_schedule integer, p_oil_pickup_service_notes text, p_oil_pickup_signature_req boolean, p_oil_tank_size double precision, p_vehicle_size integer) OWNER TO postgres;

--
-- TOC entry 222 (class 1255 OID 174035)
-- Name: insert_route(integer, interval, integer, timestamp with time zone, double precision, integer, character varying, double precision, uuid); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_route(p_batch_id integer, p_total_time interval DEFAULT NULL::interval, p_origin_location_id integer DEFAULT NULL::integer, p_route_date timestamp with time zone DEFAULT NULL::timestamp with time zone, p_distance_miles double precision DEFAULT NULL::double precision, p_vehicle_id integer DEFAULT NULL::integer, p_maps_url character varying DEFAULT NULL::character varying, p_average_location_distance_miles double precision DEFAULT NULL::double precision, p_activity_id uuid DEFAULT NULL::uuid) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO route
           (id
           ,batch_id
           ,total_time
           ,origin_location_id
           ,route_date
           ,distance_miles
           ,vehicle_id
           ,maps_url
		   ,average_location_distance_miles
		   ,activity_id
           )
     VALUES
           (DEFAULT
           ,p_batch_id
           ,p_total_time
           ,p_origin_location_id
           ,p_route_date
           ,p_distance_miles
           ,p_vehicle_id
           ,p_maps_url
		   ,p_average_location_distance_miles
		   ,p_activity_id
           );
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_route(p_batch_id integer, p_total_time interval, p_origin_location_id integer, p_route_date timestamp with time zone, p_distance_miles double precision, p_vehicle_id integer, p_maps_url character varying, p_average_location_distance_miles double precision, p_activity_id uuid) OWNER TO postgres;

--
-- TOC entry 272 (class 1255 OID 174036)
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
-- TOC entry 241 (class 1255 OID 174037)
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
-- TOC entry 274 (class 1255 OID 174038)
-- Name: insert_vehicle(character varying, character varying, double precision, boolean, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.insert_vehicle(p_name character varying DEFAULT NULL::character varying, p_model character varying DEFAULT NULL::character varying, p_oil_tank_size double precision DEFAULT NULL::double precision, p_operational boolean DEFAULT true, p_physical_size integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO vehicle
           (id
           ,name
           ,model
           ,oil_tank_size
           ,operational
           ,physical_size)
     VALUES
           (DEFAULT
           ,p_name
           ,p_model
           ,p_oil_tank_size
           ,p_operational
           ,p_physical_size);
     RETURN 1;
END;

$$;


ALTER FUNCTION public.insert_vehicle(p_name character varying, p_model character varying, p_oil_tank_size double precision, p_operational boolean, p_physical_size integer) OWNER TO postgres;

--
-- TOC entry 261 (class 1255 OID 174039)
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
-- TOC entry 198 (class 1259 OID 174040)
-- Name: config; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.config (
    id integer NOT NULL,
    current_fill_level_error_margin double precision,
    minimum_days_until_pickup integer DEFAULT 0,
    oil_pickup_average_duration interval,
    grease_pickup_average_duration interval,
    origin_location_id integer,
    google_api_key character varying,
    google_directions_maps_url character varying,
    google_api_illegal_characters character(1)[],
    maximum_days_overdue integer,
    max_distance_from_depot double precision,
    search_minimum_distance double precision,
    search_radius_percent double precision,
    genetic_algorithm_iterations integer DEFAULT 100,
    genetic_algorithm_population_size integer DEFAULT 100,
    genetic_algorithm_neighbor_count integer DEFAULT 100,
    genetic_algorithm_tournament_size integer DEFAULT 10,
    genetic_algorithm_tournament_winner_count integer DEFAULT 1,
    genetic_algorithm_breeder_count integer DEFAULT 4,
    genetic_algorithm_offspring_pool_size integer DEFAULT 2,
    genetic_algorithm_crossover_probability double precision DEFAULT 0.25,
    genetic_algorithm_elitism_ratio double precision DEFAULT 0.001,
    genetic_algorithm_mutation_probability double precision DEFAULT 0.01,
    genetic_algorithm_mutation_allele_max integer DEFAULT 1,
    genetic_algorithm_growth_decay_exponent double precision DEFAULT 1,
    grease_pickup_time_cutoff time without time zone,
    workday_start_time time without time zone,
    workday_end_time time without time zone
);


ALTER TABLE public.config OWNER TO postgres;

--
-- TOC entry 264 (class 1255 OID 174059)
-- Name: select_config(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_config() RETURNS SETOF public.config
    LANGUAGE sql
    AS $$

	SELECT * FROM config;

$$;


ALTER FUNCTION public.select_config() OWNER TO postgres;

--
-- TOC entry 245 (class 1255 OID 174060)
-- Name: select_days_until_due(date, numeric); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_days_until_due(p_last_visited date, p_oil_pickup_schedule numeric) RETURNS numeric
    LANGUAGE sql
    AS $$

select round((p_oil_pickup_schedule - EXTRACT(days FROM(now() - p_last_visited))::numeric),2);

$$;


ALTER FUNCTION public.select_days_until_due(p_last_visited date, p_oil_pickup_schedule numeric) OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 174061)
-- Name: features; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.features (
    id bigint NOT NULL,
    feature_name character varying NOT NULL,
    enabled boolean NOT NULL
);


ALTER TABLE public.features OWNER TO postgres;

--
-- TOC entry 224 (class 1255 OID 174067)
-- Name: select_features(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_features() RETURNS SETOF public.features
    LANGUAGE sql
    AS $$

	SELECT * FROM features;

$$;


ALTER FUNCTION public.select_features() OWNER TO postgres;

--
-- TOC entry 239 (class 1255 OID 174068)
-- Name: select_highest_priority_location(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_highest_priority_location() RETURNS integer
    LANGUAGE sql
    AS $$
select 1 from location;
$$;


ALTER FUNCTION public.select_highest_priority_location() OWNER TO postgres;

--
-- TOC entry 200 (class 1259 OID 174069)
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
-- TOC entry 201 (class 1259 OID 174071)
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
    route_distance_std_dev double precision DEFAULT 0,
    iteration_current integer DEFAULT 0,
    iteration_total integer,
    cancellation_request boolean DEFAULT false
);


ALTER TABLE public.route_batch OWNER TO postgres;

--
-- TOC entry 275 (class 1255 OID 174079)
-- Name: select_latest_route_batch(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_latest_route_batch() RETURNS SETOF public.route_batch
    LANGUAGE sql
    AS $$

	SELECT * FROM route_batch
    order by date_started desc NULLS LAST
	limit 1;

$$;


ALTER FUNCTION public.select_latest_route_batch() OWNER TO postgres;

--
-- TOC entry 202 (class 1259 OID 174080)
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
-- TOC entry 203 (class 1259 OID 174082)
-- Name: location; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location (
    id integer DEFAULT nextval('public.location_id_seq'::regclass) NOT NULL,
    intended_pickup_date date,
    client_priority integer DEFAULT 1 NOT NULL,
    address character varying NOT NULL,
    account character varying,
    oil_tank_size double precision,
    days_until_due double precision,
    oil_pickup_schedule integer DEFAULT 30,
    distance_from_source double precision,
    contact_name character varying,
    contact_email character varying,
    vehicle_size integer DEFAULT 10,
    grease_trap_preferred_time_end time without time zone,
    grease_trap_preferred_time_start time without time zone,
    location_type integer,
    oil_pickup_next_date date,
    oil_pickup_customer boolean DEFAULT false,
    grease_trap_customer boolean DEFAULT false,
    oil_pickup_service_notes character varying,
    oil_pickup_signature_req boolean DEFAULT false,
    status boolean,
    grease_trap_pickup_next_date date,
    grease_trap_preferred_day character varying,
    grease_trap_schedule integer DEFAULT 30,
    grease_trap_service_notes character varying,
    grease_trap_signature_req boolean DEFAULT false,
    grease_trap_size double precision,
    grease_trap_units integer,
    number_of_manholes integer,
    tracking_number bigint,
    cartesian_x double precision,
    cartesian_y double precision,
    cartesian_z double precision,
    coordinates_latitude double precision,
    coordinates_longitude double precision,
    CONSTRAINT "oil_tank_size ge 0" CHECK ((oil_tank_size >= (0)::double precision))
);


ALTER TABLE public.location OWNER TO postgres;

--
-- TOC entry 255 (class 1255 OID 174098)
-- Name: select_location(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location() RETURNS SETOF public.location
    LANGUAGE sql
    AS $$

	SELECT * FROM location order by id;

$$;


ALTER FUNCTION public.select_location() OWNER TO postgres;

--
-- TOC entry 283 (class 1255 OID 174099)
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
-- TOC entry 221 (class 1255 OID 174100)
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
-- TOC entry 204 (class 1259 OID 174101)
-- Name: location_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location_type (
    id integer NOT NULL,
    type text NOT NULL
);


ALTER TABLE public.location_type OWNER TO postgres;

--
-- TOC entry 248 (class 1255 OID 174107)
-- Name: select_location_types(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_types() RETURNS SETOF public.location_type
    LANGUAGE sql
    AS $$

	SELECT * FROM location_type order by type;

$$;


ALTER FUNCTION public.select_location_types() OWNER TO postgres;

--
-- TOC entry 277 (class 1255 OID 174108)
-- Name: select_location_with_filter(character varying, character varying, character varying, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_with_filter(p_column_filter_string character varying DEFAULT 'account'::character varying, p_filter_string character varying DEFAULT NULL::character varying, p_column_sort_string character varying DEFAULT NULL::character varying, p_ascending boolean DEFAULT NULL::boolean) RETURNS SETOF public.location
    LANGUAGE plpgsql
    AS $_$

BEGIN
	IF p_ascending is not false
		THEN
			IF p_filter_string is null and p_column_sort_string is not null
				THEN
					RETURN QUERY EXECUTE format ('SELECT * FROM location order by ' || $3 || ',id');
				ELSE IF p_filter_string is not null and p_column_sort_string is null 
					THEN
						RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by id');
				ELSE IF p_filter_string is null and p_column_sort_string is null
					THEN
						RETURN QUERY EXECUTE format ('Select * FROM location order by id');
				ELSE
					RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $3 || ', id NULLS LAST');
			END IF;
			END IF;
		END IF;

	ELSE
			IF p_filter_string is null and p_column_sort_string is not null
				THEN
					RETURN QUERY EXECUTE format ('SELECT * FROM location order by ' || $3 || ' desc, id ');
				ELSE IF p_filter_string is not null and p_column_sort_string is null 
					THEN
						RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by id desc');
				ELSE IF p_filter_string is null and p_column_sort_string is null
					THEN
						RETURN QUERY EXECUTE format ('Select * FROM location order by id desc');
				ELSE
					RETURN QUERY EXECUTE format ('Select * FROM location where ' || $1 || ' ILIKE ''%%' || $2 || '%%'' order by ' || $3 || ' desc, id NULLS LAST');
				END IF;
			END IF;
			END IF;
	END IF;

END;

$_$;


ALTER FUNCTION public.select_location_with_filter(p_column_filter_string character varying, p_filter_string character varying, p_column_sort_string character varying, p_ascending boolean) OWNER TO postgres;

--
-- TOC entry 214 (class 1259 OID 182591)
-- Name: location_with_type; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.location_with_type AS
 SELECT l.id,
    l.intended_pickup_date AS last_visited,
    l.client_priority,
    l.address,
    l.account,
    l.oil_tank_size,
    l.coordinates_latitude,
    l.coordinates_longitude,
    l.days_until_due,
    l.oil_pickup_schedule,
    l.distance_from_source,
    l.contact_name,
    l.contact_email,
    l.vehicle_size,
    l.oil_pickup_next_date,
    l.grease_trap_preferred_time_end,
    l.grease_trap_preferred_time_start,
    l.location_type AS type,
    lt.type AS type_text
   FROM (public.location l
     JOIN public.location_type lt ON ((l.location_type = lt.id)));


ALTER TABLE public.location_with_type OWNER TO postgres;

--
-- TOC entry 268 (class 1255 OID 182596)
-- Name: select_location_with_type(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_location_with_type() RETURNS SETOF public.location_with_type
    LANGUAGE sql
    AS $$

	SELECT * FROM location_with_type order by id;

$$;


ALTER FUNCTION public.select_location_with_type() OWNER TO postgres;

--
-- TOC entry 279 (class 1255 OID 174115)
-- Name: select_next_location_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_location_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM location_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_location_id() OWNER TO postgres;

--
-- TOC entry 246 (class 1255 OID 174116)
-- Name: select_next_route_batch_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_route_batch_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM route_batch_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_route_batch_id() OWNER TO postgres;

--
-- TOC entry 217 (class 1255 OID 174117)
-- Name: select_next_route_id(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_next_route_id() RETURNS bigint
    LANGUAGE sql
    AS $$

select (select last_value FROM route_id_seq) + 1;

$$;


ALTER FUNCTION public.select_next_route_id() OWNER TO postgres;

--
-- TOC entry 205 (class 1259 OID 174118)
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
-- TOC entry 206 (class 1259 OID 174120)
-- Name: route; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.route (
    id integer DEFAULT nextval('public.route_id_seq'::regclass) NOT NULL,
    origin_location_id integer,
    route_date timestamp with time zone,
    distance_miles double precision,
    total_time interval,
    maps_url character varying,
    vehicle_id integer,
    date_calculated timestamp with time zone DEFAULT now(),
    batch_id integer,
    average_location_distance_miles double precision,
    activity_id uuid
);


ALTER TABLE public.route OWNER TO postgres;

--
-- TOC entry 251 (class 1255 OID 174128)
-- Name: select_route(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route() RETURNS SETOF public.route
    LANGUAGE sql
    AS $$

	SELECT * FROM route order by id;

$$;


ALTER FUNCTION public.select_route() OWNER TO postgres;

--
-- TOC entry 271 (class 1255 OID 174129)
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
-- TOC entry 227 (class 1255 OID 174130)
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
-- TOC entry 207 (class 1259 OID 174131)
-- Name: route_location; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.route_location (
    route_id integer,
    location_id integer,
    insert_order integer
);


ALTER TABLE public.route_location OWNER TO postgres;

--
-- TOC entry 213 (class 1259 OID 182582)
-- Name: route_details; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.route_details AS
 SELECT DISTINCT r.id AS route_id,
    l.id AS location_id,
    l.account,
    l.client_priority,
    l.address,
    l.intended_pickup_date AS last_visited,
    l.days_until_due,
    l.distance_from_source,
    l.oil_pickup_customer,
    l.grease_trap_customer,
    l.coordinates_latitude,
    l.coordinates_longitude,
    r.route_date,
    r.batch_id,
    rl.insert_order
   FROM ((public.location l
     JOIN public.route_location rl ON ((rl.location_id = l.id)))
     JOIN public.route r ON ((rl.route_id = r.id)))
  WHERE (r.batch_id = public.get_latest_completed_batch_id())
  ORDER BY r.id, rl.insert_order;


ALTER TABLE public.route_details OWNER TO postgres;

--
-- TOC entry 242 (class 1255 OID 182590)
-- Name: select_route_details(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_details(p_route_id integer DEFAULT NULL::integer) RETURNS SETOF public.route_details
    LANGUAGE plpgsql
    AS $$

BEGIN
	IF p_route_id is null
    THEN
    	RETURN QUERY EXECUTE format ('SELECT * FROM route_details where batch_id = (select_next_route_batch_id() -1) order by route_id,insert_order');
    ELSE
		RETURN QUERY EXECUTE format ('SELECT * FROM route_details where route_id = ' || p_route_id);
	END IF;
END;

$$;


ALTER FUNCTION public.select_route_details(p_route_id integer) OWNER TO postgres;

--
-- TOC entry 256 (class 1255 OID 182589)
-- Name: select_route_details(integer, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_route_details(p_route_id integer DEFAULT NULL::integer, p_exclude_origin boolean DEFAULT false) RETURNS SETOF public.route_details
    LANGUAGE plpgsql
    AS $$

BEGIN
	IF p_route_id is null and p_exclude_origin = true
    THEN
    	RETURN QUERY EXECUTE format ('SELECT * FROM route_details where location_id != (select origin_location_id from config) order by route_id desc,insert_order');
	ELSE IF p_route_id is null
		THEN
		RETURN QUERY EXECUTE format ('SELECT * FROM route_details order by batch_id desc,route_id,insert_order');
    ELSE
		RETURN QUERY EXECUTE format ('SELECT * FROM route_details where route_id = ' || p_route_id);
	END IF;
	END IF;
END;

$$;


ALTER FUNCTION public.select_route_details(p_route_id integer, p_exclude_origin boolean) OWNER TO postgres;

--
-- TOC entry 208 (class 1259 OID 174141)
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
-- TOC entry 209 (class 1259 OID 174143)
-- Name: vehicle; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.vehicle (
    id integer DEFAULT nextval('public.vehicle_id_seq'::regclass) NOT NULL,
    name character varying,
    model character varying,
    oil_tank_size double precision,
    physical_size integer DEFAULT 1,
    operational boolean DEFAULT true
);


ALTER TABLE public.vehicle OWNER TO postgres;

--
-- TOC entry 212 (class 1259 OID 182577)
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
    v.id AS vehicle_id,
    v.name AS vehicle_name,
    v.model AS vehicle_model,
    r.activity_id,
    r.maps_url,
    rb.id AS batch_id
   FROM (((public.route r
     JOIN public.location l_origin ON ((r.origin_location_id = l_origin.id)))
     JOIN public.vehicle v ON ((r.vehicle_id = v.id)))
     JOIN public.route_batch rb ON ((rb.id = r.batch_id)))
  ORDER BY r.route_date;


ALTER TABLE public.route_information OWNER TO postgres;

--
-- TOC entry 237 (class 1255 OID 182588)
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
-- TOC entry 266 (class 1255 OID 174158)
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
-- TOC entry 260 (class 1255 OID 174159)
-- Name: select_vehicle(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.select_vehicle() RETURNS SETOF public.vehicle
    LANGUAGE sql
    AS $$

	SELECT * FROM vehicle order by id;


$$;


ALTER FUNCTION public.select_vehicle() OWNER TO postgres;

--
-- TOC entry 249 (class 1255 OID 174160)
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
-- TOC entry 262 (class 1255 OID 174161)
-- Name: update_days_until_due(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_days_until_due() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE location
SET days_until_due = ROUND(LEAST((EXTRACT(epoch FROM(oil_pickup_next_date - now()))/86400)::numeric
						   ,(EXTRACT(epoch FROM(grease_trap_pickup_next_date - now()))/86400)::numeric),2);

RETURN true;
END;

$$;


ALTER FUNCTION public.update_days_until_due() OWNER TO postgres;

--
-- TOC entry 254 (class 1255 OID 174162)
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
-- TOC entry 263 (class 1255 OID 174163)
-- Name: update_grease_cutoff_to_config_value(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_grease_cutoff_to_config_value() RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE location l
           set grease_trap_preferred_time_end = (select grease_pickup_time_cutoff from config)::interval 
		   FROM location_type lt
           where lt.id = l.Location_type and lt.type = 'grease' and l.grease_trap_preferred_time_end is null;
           return 1;         
END;

$$;


ALTER FUNCTION public.update_grease_cutoff_to_config_value() OWNER TO postgres;

--
-- TOC entry 284 (class 1255 OID 174164)
-- Name: update_iteration(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_iteration(p_iteration_current integer, p_iteration_total integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

 
BEGIN
Update route_batch 
SET iteration_current = p_iteration_current,iteration_total=p_iteration_total
where id = (select id FROM route_batch ORDER BY id DESC LIMIT 1);
return 1;
END;
$$;


ALTER FUNCTION public.update_iteration(p_iteration_current integer, p_iteration_total integer) OWNER TO postgres;

--
-- TOC entry 228 (class 1255 OID 182312)
-- Name: update_location(integer, integer, date, integer, time without time zone, time without time zone, character varying, character varying, integer, double precision, double precision, double precision, double precision, character varying, character varying, integer, integer, date, boolean, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_location(p_id integer, p_client_priority integer DEFAULT NULL::integer, p_intended_pickup_date date DEFAULT NULL::date, p_oil_pickup_schedule integer DEFAULT NULL::integer, p_grease_trap_preferred_time_start time without time zone DEFAULT NULL::time without time zone, p_grease_trap_preferred_time_end time without time zone DEFAULT NULL::time without time zone, p_address character varying DEFAULT NULL::character varying, p_account character varying DEFAULT NULL::character varying, p_oil_tank_size integer DEFAULT NULL::integer, p_coordinates_latitude double precision DEFAULT NULL::double precision, p_coordinates_longitude double precision DEFAULT NULL::double precision, p_days_until_due double precision DEFAULT NULL::double precision, p_distance_from_source double precision DEFAULT NULL::double precision, p_contact_name character varying DEFAULT NULL::character varying, p_contact_email character varying DEFAULT NULL::character varying, p_vehicle_size integer DEFAULT NULL::integer, p_location_type integer DEFAULT NULL::integer, p_oil_pickup_next_date date DEFAULT NULL::date, p_oil_pickup_customer boolean DEFAULT NULL::boolean, p_grease_trap_customer boolean DEFAULT NULL::boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE location	   
           SET 
            client_priority = COALESCE(p_client_priority, client_priority)
           ,intended_pickup_date = COALESCE(p_intended_pickup_date, intended_pickup_date)
           ,oil_pickup_schedule = COALESCE(p_oil_pickup_schedule, oil_pickup_schedule)
		   ,grease_trap_preferred_time_start = COALESCE(p_grease_trap_preferred_time_start, grease_trap_preferred_time_start)
		   ,grease_trap_preferred_time_end = COALESCE(p_grease_trap_preferred_time_end, grease_trap_preferred_time_end)
           ,address = COALESCE(p_address,address)
           ,account = COALESCE(p_account,account)
           ,oil_tank_size = COALESCE(p_oil_tank_size,oil_tank_size)
           ,coordinates_latitude = COALESCE(p_coordinates_latitude,coordinates_latitude)
	       ,coordinates_longitude = COALESCE(p_coordinates_longitude,coordinates_longitude)
           ,days_until_due = COALESCE(p_days_until_due,days_until_due) 
           ,distance_from_source = COALESCE(p_distance_from_source,distance_from_source) 
           ,contact_name = COALESCE(p_contact_name,contact_name)
           ,contact_email = COALESCE(p_contact_email,contact_email)
           ,vehicle_size = COALESCE(p_vehicle_size,vehicle_size)
		   ,location_type = COALESCE(p_location_type,location_type)  
           ,oil_pickup_next_date = COALESCE(p_oil_pickup_next_date,oil_pickup_next_date)     
		   ,oil_pickup_customer = COALESCE(p_oil_pickup_customer,oil_pickup_customer)
	       ,grease_trap_customer = COALESCE(p_grease_trap_customer,grease_trap_customer)

           WHERE id = p_id;
           return 1;

END;

$$;


ALTER FUNCTION public.update_location(p_id integer, p_client_priority integer, p_intended_pickup_date date, p_oil_pickup_schedule integer, p_grease_trap_preferred_time_start time without time zone, p_grease_trap_preferred_time_end time without time zone, p_address character varying, p_account character varying, p_oil_tank_size integer, p_coordinates_latitude double precision, p_coordinates_longitude double precision, p_days_until_due double precision, p_distance_from_source double precision, p_contact_name character varying, p_contact_email character varying, p_vehicle_size integer, p_location_type integer, p_oil_pickup_next_date date, p_oil_pickup_customer boolean, p_grease_trap_customer boolean) OWNER TO postgres;

--
-- TOC entry 233 (class 1255 OID 174166)
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
-- TOC entry 232 (class 1255 OID 174167)
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
-- TOC entry 230 (class 1255 OID 174168)
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
-- TOC entry 238 (class 1255 OID 174169)
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
-- TOC entry 236 (class 1255 OID 174170)
-- Name: update_route_batch_cancellation_status(boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_route_batch_cancellation_status(p_cancellation_request boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE route_batch   
   SET 
   cancellation_request = p_cancellation_request
   WHERE id = (SELECT id FROM route_batch 
		order by id desc limit 1);
   return 1;
END;

$$;


ALTER FUNCTION public.update_route_batch_cancellation_status(p_cancellation_request boolean) OWNER TO postgres;

--
-- TOC entry 234 (class 1255 OID 174171)
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
-- TOC entry 219 (class 1255 OID 174172)
-- Name: update_route_location(integer, integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_route_location(p_location_id integer, p_route_id integer, p_order integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

DECLARE
latest_batch_id bigint = get_latest_completed_batch_id();
actual_order integer = GREATEST(LEAST((select count (1) from route_location as rl INNER JOIN route as r ON p_route_id = r.id Where r.batch_id = latest_batch_id and route_id = p_route_id) - 1, p_order),2);
original_route_id bigint = (select route_id from route_details where location_id = p_location_id);
original_order integer = (select insert_order from route_details where location_id = p_location_id);
origin_start integer = (select insert_order from route_location as rl INNER JOIN route as r ON p_route_id = r.id Where r.batch_id = latest_batch_id and route_id = p_route_id order by insert_order asc limit 1);
origin_end integer = (select insert_order from route_location as rl INNER JOIN route as r ON p_route_id = r.id Where r.batch_id = latest_batch_id and route_id = p_route_id order by insert_order desc limit 1);
BEGIN

if (original_route_id = p_route_id and actual_order > original_order)
THEN
UPDATE route_location
	SET insert_order = insert_order - 1
	FROM route as r
	WHERE route_id = p_route_id and (insert_order > original_order and insert_order <= actual_order) and location_id != p_location_id and (insert_order != origin_start and insert_order != origin_end) and (r.batch_id = latest_batch_id);
END IF;

if (original_route_id = p_route_id and actual_order < original_order)
THEN
UPDATE route_location as rl
	SET insert_order = insert_order + 1
	FROM route as r
	WHERE (route_id = p_route_id and (insert_order >= actual_order and insert_order < original_order) and location_id != p_location_id)  and (insert_order != origin_start and insert_order != origin_end) and (r.batch_id = latest_batch_id);
END IF;

UPDATE route_location as rl
SET route_id = p_route_id,insert_order = actual_order
FROM route as r
WHERE rl.location_id = p_location_id and (r.batch_id = latest_batch_id);

if (original_route_id != p_route_id)
THEN
UPDATE route_location as rl
	SET insert_order = insert_order - 1
	FROM route as r
	WHERE (route_id = original_route_id and insert_order > original_order) and (location_id != p_location_id) and (r.batch_id = latest_batch_id);
END IF;

if (original_route_id != p_route_id)
THEN
UPDATE route_location as rl
	SET insert_order = insert_order + 1
	FROM route as r
	WHERE (route_id = p_route_id and insert_order >= p_order) and (insert_order != origin_start) and (location_id != p_location_id) and (r.batch_id = latest_batch_id);
END IF;

if (original_route_id != p_route_id and p_order > actual_order)
THEN
UPDATE route_location as rl
	SET insert_order = insert_order + 1
	FROM route as r
	WHERE (location_id = p_location_id or insert_order = origin_end) and (r.batch_id = latest_batch_id);
END IF;

return 1;

END;

$$;


ALTER FUNCTION public.update_route_location(p_location_id integer, p_route_id integer, p_order integer) OWNER TO postgres;

--
-- TOC entry 244 (class 1255 OID 174173)
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
-- TOC entry 265 (class 1255 OID 174174)
-- Name: update_vehicle(integer, character varying, character varying, double precision, boolean, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.update_vehicle(p_id integer, p_name character varying DEFAULT NULL::character varying, p_model character varying DEFAULT NULL::character varying, p_oil_tank_size double precision DEFAULT NULL::double precision, p_operational boolean DEFAULT NULL::boolean, p_physical_size integer DEFAULT NULL::integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
UPDATE vehicle   
           SET 
            name = COALESCE(p_name, name)
           ,model = COALESCE(p_model, model)
           ,oil_tank_size = COALESCE(p_oil_tank_size, oil_tank_size)
           ,operational = COALESCE(p_operational,operational)
           ,physical_size = COALESCE(p_physical_size,physical_size)
           WHERE id = p_id;
           return 1;

           
END;

$$;


ALTER FUNCTION public.update_vehicle(p_id integer, p_name character varying, p_model character varying, p_oil_tank_size double precision, p_operational boolean, p_physical_size integer) OWNER TO postgres;

--
-- TOC entry 231 (class 1255 OID 174175)
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
-- TOC entry 243 (class 1255 OID 182254)
-- Name: upsert_config(integer, integer, double precision, double precision, integer, time without time zone, time without time zone, time without time zone, interval, interval, character varying, character varying, character[], double precision, double precision, integer, integer, integer, integer, integer, integer, integer, double precision, double precision, double precision, integer, double precision); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.upsert_config(p_origin_location_id integer DEFAULT NULL::integer, p_minimum_days_until_pickup integer DEFAULT NULL::integer, p_current_fill_level_error_margin double precision DEFAULT NULL::double precision, p_max_distance_from_depot double precision DEFAULT NULL::double precision, p_maximum_days_overdue integer DEFAULT NULL::integer, p_workday_start_time time without time zone DEFAULT NULL::time without time zone, p_workday_end_time time without time zone DEFAULT NULL::time without time zone, p_grease_pickup_time_cutoff time without time zone DEFAULT NULL::time without time zone, p_oil_pickup_average_duration interval DEFAULT NULL::interval, p_grease_pickup_average_duration interval DEFAULT NULL::interval, p_google_directions_maps_url character varying DEFAULT NULL::character varying, p_google_api_key character varying DEFAULT NULL::character varying, p_google_api_illegal_characters character[] DEFAULT NULL::character(1)[], p_search_minimum_distance double precision DEFAULT NULL::double precision, p_search_radius_percent double precision DEFAULT NULL::double precision, p_genetic_algorithm_iterations integer DEFAULT NULL::integer, p_genetic_algorithm_population_size integer DEFAULT NULL::integer, p_genetic_algorithm_neighbor_count integer DEFAULT NULL::integer, p_genetic_algorithm_tournament_size integer DEFAULT NULL::integer, p_genetic_algorithm_tournament_winner_count integer DEFAULT NULL::integer, p_genetic_algorithm_breeder_count integer DEFAULT NULL::integer, p_genetic_algorithm_offspring_pool_size integer DEFAULT NULL::integer, p_genetic_algorithm_crossover_probability double precision DEFAULT NULL::double precision, p_genetic_algorithm_elitism_ratio double precision DEFAULT NULL::double precision, p_genetic_algorithm_mutation_probability double precision DEFAULT NULL::double precision, p_genetic_algorithm_mutation_allele_max integer DEFAULT NULL::integer, p_genetic_algorithm_growth_decay_exponent double precision DEFAULT NULL::double precision) RETURNS boolean
    LANGUAGE plpgsql
    AS $$

BEGIN
INSERT INTO config 
(
    id
,origin_location_id
,minimum_days_until_pickup
,current_fill_level_error_margin
,max_distance_from_depot
,maximum_days_overdue
,workday_start_time
,workday_end_time
,grease_pickup_time_cutoff
,oil_pickup_average_duration
,grease_pickup_average_duration
,google_directions_maps_url
,google_api_key 
,google_api_illegal_characters
,search_minimum_distance
,search_radius_percent
,genetic_algorithm_iterations
,genetic_algorithm_population_size
,genetic_algorithm_neighbor_count
,genetic_algorithm_tournament_size
,genetic_algorithm_tournament_winner_count
,genetic_algorithm_breeder_count
,genetic_algorithm_offspring_pool_size
,genetic_algorithm_crossover_probability
,genetic_algorithm_elitism_ratio
,genetic_algorithm_mutation_probability
,genetic_algorithm_mutation_allele_max
,genetic_algorithm_growth_decay_exponent
)
VALUES
(
1
,p_origin_location_id
,p_minimum_days_until_pickup
,p_current_fill_level_error_margin
,p_max_distance_from_depot
,p_maximum_days_overdue
,p_workday_start_time
,p_workday_end_time
,p_grease_pickup_time_cutoff
,p_oil_pickup_average_duration
,p_grease_pickup_average_duration
,p_google_directions_maps_url
,p_google_api_key
,p_google_api_illegal_characters
,p_search_minimum_distance
,p_search_radius_percent
,p_genetic_algorithm_iterations
,p_genetic_algorithm_population_size
,p_genetic_algorithm_neighbor_count
,p_genetic_algorithm_tournament_size
,p_genetic_algorithm_tournament_winner_count
,p_genetic_algorithm_breeder_count
,p_genetic_algorithm_offspring_pool_size
,p_genetic_algorithm_crossover_probability
,p_genetic_algorithm_elitism_ratio
,p_genetic_algorithm_mutation_probability
,p_genetic_algorithm_mutation_allele_max
,p_genetic_algorithm_growth_decay_exponent
)
    ON CONFLICT (id)
DO UPDATE    
           SET 
           origin_location_id = COALESCE(p_origin_location_id, config.origin_location_id),
           minimum_days_until_pickup = COALESCE(p_minimum_days_until_pickup, config.minimum_days_until_pickup),
           current_fill_level_error_margin = COALESCE(p_current_fill_level_error_margin, config.current_fill_level_error_margin),
		   max_distance_from_depot = COALESCE(p_max_distance_from_depot, config.max_distance_from_depot),
		   maximum_days_overdue = COALESCE(p_maximum_days_overdue, config.maximum_days_overdue),
           workday_start_time = COALESCE(p_workday_start_time, config.workday_start_time),
		   workday_end_time = COALESCE(p_workday_end_time, config.workday_end_time),
		   grease_pickup_time_cutoff = COALESCE(p_grease_pickup_time_cutoff, config.grease_pickup_time_cutoff),
		   oil_pickup_average_duration = COALESCE(p_oil_pickup_average_duration, config.oil_pickup_average_duration),
           grease_pickup_average_duration = COALESCE(p_grease_pickup_average_duration, config.grease_pickup_average_duration),
		   google_directions_maps_url = COALESCE(p_google_directions_maps_url, config.google_directions_maps_url),
		   google_api_key = COALESCE(p_google_api_key, config.google_api_key),
		   google_api_illegal_characters = COALESCE(p_google_api_illegal_characters, config.google_api_illegal_characters),
		   search_minimum_distance =COALESCE(p_search_minimum_distance, config.search_minimum_distance),
		   search_radius_percent =COALESCE(p_search_radius_percent, config.search_radius_percent),
		   genetic_algorithm_iterations = COALESCE(p_genetic_algorithm_iterations, config.genetic_algorithm_iterations),
		   genetic_algorithm_population_size = COALESCE(p_genetic_algorithm_population_size, config.genetic_algorithm_population_size),
		   genetic_algorithm_neighbor_count = COALESCE(p_genetic_algorithm_neighbor_count, config.genetic_algorithm_neighbor_count),
		   genetic_algorithm_tournament_size = COALESCE(p_genetic_algorithm_tournament_size, config.genetic_algorithm_tournament_size),
		   genetic_algorithm_tournament_winner_count = COALESCE(p_genetic_algorithm_tournament_winner_count, config.genetic_algorithm_tournament_winner_count),
		   genetic_algorithm_breeder_count = COALESCE(p_genetic_algorithm_breeder_count, config.genetic_algorithm_breeder_count),
		   genetic_algorithm_offspring_pool_size = COALESCE(p_genetic_algorithm_offspring_pool_size, config.genetic_algorithm_offspring_pool_size),
		   genetic_algorithm_crossover_probability = COALESCE(p_genetic_algorithm_crossover_probability, config.genetic_algorithm_crossover_probability),
		   genetic_algorithm_elitism_ratio = COALESCE(p_genetic_algorithm_elitism_ratio, config.genetic_algorithm_elitism_ratio),
		   genetic_algorithm_mutation_probability = COALESCE(p_genetic_algorithm_mutation_probability, config.genetic_algorithm_mutation_probability),
		   genetic_algorithm_mutation_allele_max = COALESCE(p_genetic_algorithm_mutation_allele_max, config.genetic_algorithm_mutation_allele_max),
		   genetic_algorithm_growth_decay_exponent = COALESCE(p_genetic_algorithm_growth_decay_exponent, config.genetic_algorithm_growth_decay_exponent)

		   ;
           return 1;

END;

$$;


ALTER FUNCTION public.upsert_config(p_origin_location_id integer, p_minimum_days_until_pickup integer, p_current_fill_level_error_margin double precision, p_max_distance_from_depot double precision, p_maximum_days_overdue integer, p_workday_start_time time without time zone, p_workday_end_time time without time zone, p_grease_pickup_time_cutoff time without time zone, p_oil_pickup_average_duration interval, p_grease_pickup_average_duration interval, p_google_directions_maps_url character varying, p_google_api_key character varying, p_google_api_illegal_characters character[], p_search_minimum_distance double precision, p_search_radius_percent double precision, p_genetic_algorithm_iterations integer, p_genetic_algorithm_population_size integer, p_genetic_algorithm_neighbor_count integer, p_genetic_algorithm_tournament_size integer, p_genetic_algorithm_tournament_winner_count integer, p_genetic_algorithm_breeder_count integer, p_genetic_algorithm_offspring_pool_size integer, p_genetic_algorithm_crossover_probability double precision, p_genetic_algorithm_elitism_ratio double precision, p_genetic_algorithm_mutation_probability double precision, p_genetic_algorithm_mutation_allele_max integer, p_genetic_algorithm_growth_decay_exponent double precision) OWNER TO postgres;

--
-- TOC entry 210 (class 1259 OID 174179)
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
-- TOC entry 211 (class 1259 OID 174181)
-- Name: api_metadata; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.api_metadata (
    id integer DEFAULT nextval('public.api_metadata_id_seq'::regclass) NOT NULL,
    call_date date NOT NULL,
    api_call_count integer
);


ALTER TABLE public.api_metadata OWNER TO postgres;

--
-- TOC entry 2846 (class 2606 OID 174186)
-- Name: api_metadata call_date_unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.api_metadata
    ADD CONSTRAINT call_date_unique UNIQUE (call_date);


--
-- TOC entry 2826 (class 2606 OID 174188)
-- Name: config config_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.config
    ADD CONSTRAINT config_pkey PRIMARY KEY (id);


--
-- TOC entry 2828 (class 2606 OID 174190)
-- Name: features features_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.features
    ADD CONSTRAINT features_pkey PRIMARY KEY (id);


--
-- TOC entry 2836 (class 2606 OID 174192)
-- Name: location_type id unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_type
    ADD CONSTRAINT "id unique" UNIQUE (id);


--
-- TOC entry 2832 (class 2606 OID 174194)
-- Name: location location_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location
    ADD CONSTRAINT location_pkey PRIMARY KEY (id);


--
-- TOC entry 2838 (class 2606 OID 174196)
-- Name: location_type location_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_type
    ADD CONSTRAINT location_type_pkey PRIMARY KEY (id, type);


--
-- TOC entry 2830 (class 2606 OID 174198)
-- Name: route_batch route_batch_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route_batch
    ADD CONSTRAINT route_batch_pkey PRIMARY KEY (id);


--
-- TOC entry 2842 (class 2606 OID 174200)
-- Name: route route_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route
    ADD CONSTRAINT route_pkey PRIMARY KEY (id);


--
-- TOC entry 2834 (class 2606 OID 182635)
-- Name: location tracking_number; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location
    ADD CONSTRAINT tracking_number UNIQUE (tracking_number);


--
-- TOC entry 2840 (class 2606 OID 174202)
-- Name: location_type type unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_type
    ADD CONSTRAINT "type unique" UNIQUE (type);


--
-- TOC entry 2844 (class 2606 OID 174204)
-- Name: vehicle vehicle_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.vehicle
    ADD CONSTRAINT vehicle_pkey PRIMARY KEY (id);


--
-- TOC entry 2850 (class 2620 OID 174205)
-- Name: route_batch update_batch_route_calculation_time; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_batch_route_calculation_time AFTER UPDATE OF date_completed ON public.route_batch FOR EACH ROW EXECUTE PROCEDURE public.update_route_batch_calculation_time();


--
-- TOC entry 2851 (class 2620 OID 182599)
-- Name: location update_cartesian_values; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_cartesian_values AFTER UPDATE OF coordinates_latitude, coordinates_longitude ON public.location FOR EACH ROW WHEN (((old.coordinates_latitude IS NOT NULL) AND (old.coordinates_longitude IS NOT NULL))) EXECUTE PROCEDURE public.calculate_xyz_cartesian_from_gps();


--
-- TOC entry 2852 (class 2620 OID 174206)
-- Name: location update_maps_urls; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER update_maps_urls AFTER UPDATE OF address ON public.location FOR EACH ROW WHEN (((old.address)::text <> (new.address)::text)) EXECUTE PROCEDURE public.update_maps_urls();


--
-- TOC entry 2848 (class 2606 OID 174207)
-- Name: route_location location_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route_location
    ADD CONSTRAINT location_id FOREIGN KEY (location_id) REFERENCES public.location(id);


--
-- TOC entry 2849 (class 2606 OID 174212)
-- Name: route_location route_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.route_location
    ADD CONSTRAINT route_id FOREIGN KEY (route_id) REFERENCES public.route(id);


--
-- TOC entry 2847 (class 2606 OID 174217)
-- Name: location type; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location
    ADD CONSTRAINT type FOREIGN KEY (location_type) REFERENCES public.location_type(id);


-- Completed on 2019-02-17 02:37:17

--
-- PostgreSQL database dump complete
--

